namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using TenPercent.Application.Interfaces;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.Models;

    public class SeasonService : ISeasonService
    {
        private readonly AppDbContext _context;
        private readonly IPlayerContractService _contractService;
        private readonly IPlayerService _playerService;

        public SeasonService(AppDbContext context, IPlayerContractService contractService, IPlayerService playerService)
        {
            _context = context;
            _contractService = contractService;
            _playerService = playerService;
        }

        public async Task<(bool Success, string Message)> EndCurrentSeasonAsync()
        {
            var activeSeason = await _context.Seasons
                .Include(s => s.Fixtures)
                .FirstOrDefaultAsync(s => s.IsActive);

            // ==========================================
            // 1. ВАЛИДАЦИИ (PRE-CHECKS)
            // ==========================================
            if (activeSeason == null)
            {
                return (false, "Няма активен сезон за приключване.");
            }

            // Проверка 1: Всички мачове трябва да са изиграни
            bool hasUnplayedFixtures = activeSeason.Fixtures.Any(f => !f.IsPlayed);
            if (hasUnplayedFixtures)
            {
                int unplayedCount = activeSeason.Fixtures.Count(f => !f.IsPlayed);
                return (false, $"Не може да приключите сезона! Има {unplayedCount} неизиграни мачове. Използвайте 'Simulate Matchday', за да ги изиграете.");
            }

            // Проверка 2: Дали са генерирани изобщо мачове за този сезон? (За да не се цъкне End веднага след Start)
            if (activeSeason.Fixtures.Count == 0 && activeSeason.TotalGameweeks > 0)
            {
                return (false, "Мачовете бяха изтрити или не са генерирани правилно. Моля, свържете се с администратор.");
            }

            // TODO: (Бъдещи проверки)
            // Проверка 3: Дали трансферният прозорец е затворен и няма висящи оферти?
            // Проверка 4: Дали всички клубове не са фалирали (ако имаш система за банкрут)?

            // ==========================================
            // 2. ИЗПЪЛНЕНИЕ (EXECUTION)
            // ==========================================
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                int currentSeasonNumber = activeSeason.SeasonNumber;

                // 2.1 ЗАПЕЧАТВАНЕ НА КЛАСИРАНИЕТО
                var liveStandings = await _context.LeagueStandings.ToListAsync();
                var leagues = liveStandings.GroupBy(ls => ls.LeagueId);
                var seasonStandings = new List<SeasonStanding>();

                foreach (var leagueGroup in leagues)
                {
                    var sortedStandings = leagueGroup
                        .OrderByDescending(s => s.Points)
                        .ThenByDescending(s => s.GoalsFor - s.GoalsAgainst)
                        .ThenByDescending(s => s.GoalsFor)
                        .ToList();

                    for (int i = 0; i < sortedStandings.Count; i++)
                    {
                        var live = sortedStandings[i];
                        seasonStandings.Add(new SeasonStanding
                        {
                            SeasonId = activeSeason.Id,
                            LeagueId = live.LeagueId,
                            ClubId = live.ClubId,
                            Position = i + 1,
                            Played = live.Played,
                            Won = live.Won,
                            Drawn = live.Drawn,
                            Lost = live.Lost,
                            GoalsFor = live.GoalsFor,
                            GoalsAgainst = live.GoalsAgainst,
                            Points = live.Points,
                            IsChampion = (i == 0)
                        });

                        // Ресетваме живата таблица за следващия сезон
                        live.Played = 0; live.Won = 0; live.Drawn = 0; live.Lost = 0;
                        live.GoalsFor = 0; live.GoalsAgainst = 0; live.Points = 0;
                    }
                }
                _context.SeasonStandings.AddRange(seasonStandings);

                // 2.2 ЗАПЕЧАТВАНЕ НА СТАТИСТИКИТЕ НА ИГРАЧИТЕ
                var matchPerformances = await _context.PlayerMatchPerformances
                    .Include(pmp => pmp.Fixture)
                    .Where(pmp => pmp.Fixture.SeasonId == activeSeason.Id)
                    .ToListAsync();

                var seasonStats = new List<PlayerSeasonPerformance>();
                foreach (var group in matchPerformances.GroupBy(p => p.PlayerId))
                {
                    var stats = group.ToList();
                    var player = await _context.Players.FindAsync(group.Key);

                    seasonStats.Add(new PlayerSeasonPerformance
                    {
                        SeasonId = activeSeason.Id,
                        PlayerId = group.Key,
                        ClubId = player?.ClubId,
                        Appearances = stats.Count,
                        Goals = stats.Sum(s => s.Goals),
                        Assists = stats.Sum(s => s.Assists),
                        YellowCards = stats.Sum(s => s.YellowCards),
                        RedCards = stats.Sum(s => s.RedCards),
                        AverageRating = stats.Average(s => s.MatchRating)
                    });
                }
                _context.PlayerSeasonStats.AddRange(seasonStats);

                // 2.3 ИЗЧИСТВАНЕ НА ИЗИГРАНИТЕ МАЧОВЕ (за да не се пълни базата безкрайно)
                _context.Fixtures.RemoveRange(activeSeason.Fixtures);

                // 2.4 ЗАТВАРЯНЕ НА ТЕКУЩИЯ СЕЗОН
                activeSeason.IsActive = false;
                activeSeason.EndDate = DateTime.UtcNow;

                var worldState = await _context.WorldStates.FirstOrDefaultAsync();
                if (worldState != null) worldState.CurrentSeasonId = null;

                await _context.SaveChangesAsync();

                // ==========================================
                // 3. ПОСТ-СЕЗОННА ПРОГРЕСИЯ (POST-SEASON)
                // ==========================================

                // 3.1 Остаряване на играчите и прогресия на атрибути (+1 година)
                await _playerService.ProcessYearlyProgressionAsync();

                // 3.2 Изтичане на договорите (Клубни и Агентски)
                await _contractService.ProcessContractsYearEndAsync(currentSeasonNumber);

                await transaction.CommitAsync();

                return (true, $"Сезон {currentSeasonNumber} приключи! Класиранията са архивирани. Играчите остаряха. Изтеклите договори бяха прекратени.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Грешка при приключване на сезона: {ex.Message}");
            }
        }



        public async Task<(bool Success, string Message)> StartNewSeasonAsync()
        {
            var worldState = await _context.WorldStates.FirstOrDefaultAsync();
            if (worldState == null) return (false, "World Engine must be initialized first!");

            bool hasActive = await _context.Seasons.AnyAsync(s => s.IsActive);
            if (hasActive) return (false, "Не може да започнете нов сезон, защото има текущ активен сезон!");

            int lastSeasonNumber = await _context.Seasons.MaxAsync(s => (int?)s.SeasonNumber) ?? 0;
            int newSeasonNumber = lastSeasonNumber + 1;

            var newSeason = new Season
            {
                SeasonNumber = newSeasonNumber,
                StartDate = DateTime.UtcNow,
                IsActive = true,
                CurrentGameweek = 1,
                TotalGameweeks = 0 // Остава 0, докато не се извика GenerateSchedule
            };

            _context.Seasons.Add(newSeason);
            await _context.SaveChangesAsync(); // За да вземем Id

            // Връзваме шапката за новия сезон
            worldState.CurrentSeasonId = newSeason.Id;
            await _context.SaveChangesAsync();

            return (true, $"Сезон {newSeasonNumber} стартира успешно! Не забравяйте да генерирате програмата (Fixtures).");
        }
    }
}