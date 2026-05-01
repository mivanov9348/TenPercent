namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.Models;

    public class SeasonService : ISeasonService
    {
        private readonly AppDbContext _context;

        public SeasonService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> EndCurrentSeasonAsync()
        {
            var activeSeason = await _context.Seasons
                .Include(s => s.Fixtures)
                .FirstOrDefaultAsync(s => s.IsActive);

            if (activeSeason == null) return (false, "Няма активен сезон за приключване.");

            if (activeSeason.Fixtures.Any(f => !f.IsPlayed))
                return (false, "Не може да приключите сезона! Има неизиграни мачове.");

            // 1. ЗАПЕЧАТВАНЕ НА КЛАСИРАНИЕТО
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

                    // Зануляване
                    live.Played = 0; live.Won = 0; live.Drawn = 0; live.Lost = 0;
                    live.GoalsFor = 0; live.GoalsAgainst = 0; live.Points = 0;
                }
            }
            _context.SeasonStandings.AddRange(seasonStandings);

            // 2. ЗАПЕЧАТВАНЕ НА СТАТИСТИКИТЕ
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

            // 3. ИЗТРИВАНЕ И ЗАТВАРЯНЕ
            _context.Fixtures.RemoveRange(activeSeason.Fixtures);

            activeSeason.IsActive = false;
            activeSeason.EndDate = DateTime.UtcNow;

            var worldState = await _context.WorldStates.FirstOrDefaultAsync();
            if (worldState != null) worldState.CurrentSeasonId = null; // Освобождаваме шапката

            await _context.SaveChangesAsync();

            return (true, $"Сезон {activeSeason.SeasonNumber} приключи успешно! Класиранията и статистиките са архивирани.");
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