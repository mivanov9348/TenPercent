namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Application.Interfaces;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.Models;

    public class SimulationService : ISimulationService
    {
        private readonly AppDbContext _context;
        private readonly IMatchEngineService _matchEngine;
        private readonly IFinanceService _financeService;
        public SimulationService(AppDbContext context, IMatchEngineService matchEngine, IFinanceService financeService)
        {
            _context = context;
            _matchEngine = matchEngine;
            _financeService = financeService;
        }

        public async Task<(bool Success, string Message)> SimulateNextGameweekAsync()
        {
            var worldState = await _context.WorldStates.FirstOrDefaultAsync();
            if (worldState == null || worldState.CurrentSeasonId == null)
                return (false, "Светът не е инициализиран или няма активен сезон.");

            var activeSeason = await _context.Seasons.FirstOrDefaultAsync(s => s.Id == worldState.CurrentSeasonId);
            if (activeSeason == null || !activeSeason.IsActive)
                return (false, "Няма активен сезон за симулиране.");

            if (activeSeason.CurrentGameweek > activeSeason.TotalGameweeks)
                return (false, "Сезонът вече е изигран изцяло. Приключете го от Админ панела.");

            // 1. Взимаме всички мачове за текущия Gameweek (Заедно с играчите и позициите им)
            var fixturesToPlay = await _context.Fixtures
                .Include(f => f.HomeClub).ThenInclude(c => c.Players).ThenInclude(p => p.Attributes)
                .Include(f => f.HomeClub).ThenInclude(c => c.Players).ThenInclude(p => p.Position) // ВАЖНО: Трябва ни Position за Engine-а
                .Include(f => f.AwayClub).ThenInclude(c => c.Players).ThenInclude(p => p.Attributes)
                .Include(f => f.AwayClub).ThenInclude(c => c.Players).ThenInclude(p => p.Position)
                .Where(f => f.SeasonId == activeSeason.Id && f.Gameweek == activeSeason.CurrentGameweek && !f.IsPlayed)
                .ToListAsync();

            if (!fixturesToPlay.Any())
                return (false, $"Няма намерени мачове за изиграване в Gameweek {activeSeason.CurrentGameweek}.");

            // 2. Взимаме живото класиране за ъпдейт
            var standings = await _context.LeagueStandings.ToListAsync();

            // 3. НОВО: Дърпаме всички съществуващи сезонни статистики в паметта като Dictionary (за светкавичен достъп)
            var seasonStatsDict = await _context.PlayerSeasonStats
                .Where(ps => ps.SeasonId == activeSeason.Id)
                .ToDictionaryAsync(ps => ps.PlayerId);

            // 4. Симулираме всеки мач
            foreach (var match in fixturesToPlay)
            {
                var playedMatch = await _matchEngine.PlayMatchAsync(match);

                // 4.1. Ъпдейтваме отборното класиране
                UpdateStandings(standings, playedMatch);

                // 4.2. НОВО: Ъпдейтваме индивидуалните статистики за сезона!
                UpdatePlayerSeasonStats(seasonStatsDict, playedMatch, activeSeason.Id);
            }

            // 5. Увеличаваме Gameweek на Сезона с 1
            activeSeason.CurrentGameweek++;

            // 6. Обновяваме датата на следващия мач
            worldState.NextMatchdayDate = DateTime.UtcNow.AddDays(1);

            // 7. Запазваме всичко наведнъж (Мачове, Класирания, MatchPerformances и SeasonPerformances)
            await _context.SaveChangesAsync();

            var financeResult = await _financeService.ProcessWeeklyWagesAsync();

            string finalMessage = $"Gameweek {activeSeason.CurrentGameweek - 1} симулиран успешно! Изиграни {fixturesToPlay.Count} мача.";

            // Добавяме финансовия репорт към крайното съобщение, за да го виждаш в Postman/Swagger
            if (financeResult.Success)
            {
                finalMessage += $"\nФинансов отчет: {financeResult.Message}";
            }
            else
            {
                finalMessage += $"\nВНИМАНИЕ: Проблем с финансите: {financeResult.Message}";
            }

            return (true, finalMessage);
        }

        private void UpdateStandings(List<LeagueStanding> standings, Fixture match)
        {
            var homeStanding = standings.First(s => s.LeagueId == match.LeagueId && s.ClubId == match.HomeClubId);
            var awayStanding = standings.First(s => s.LeagueId == match.LeagueId && s.ClubId == match.AwayClubId);

            homeStanding.Played++;
            awayStanding.Played++;
            homeStanding.GoalsFor += match.HomeGoals;
            homeStanding.GoalsAgainst += match.AwayGoals;
            awayStanding.GoalsFor += match.AwayGoals;
            awayStanding.GoalsAgainst += match.HomeGoals;

            if (match.HomeGoals > match.AwayGoals)
            {
                homeStanding.Won++;
                homeStanding.Points += 3;
                awayStanding.Lost++;
            }
            else if (match.HomeGoals < match.AwayGoals)
            {
                awayStanding.Won++;
                awayStanding.Points += 3;
                homeStanding.Lost++;
            }
            else
            {
                homeStanding.Drawn++;
                homeStanding.Points += 1;
                awayStanding.Drawn++;
                awayStanding.Points += 1;
            }
        }

        // --- НОВИЯТ МЕТОД ЗА СТАТИСТИКИТЕ ---
        private void UpdatePlayerSeasonStats(Dictionary<int, PlayerSeasonPerformance> seasonStatsDict, Fixture match, int seasonId)
        {
            foreach (var matchPerf in match.Performances)
            {
                // Засега броим всяко участие с минути > 0 за Appearance.
                // В бъдеще, ако имаме резерви, които не влизат в игра, това ще ги филтрира.
                if (matchPerf.MinutesPlayed == 0) continue;

                // Проверяваме дали играчът вече има създаден запис за този сезон
                if (!seasonStatsDict.TryGetValue(matchPerf.PlayerId, out var seasonStat))
                {
                    // Ако няма, създаваме нов
                    seasonStat = new PlayerSeasonPerformance
                    {
                        SeasonId = seasonId,
                        PlayerId = matchPerf.PlayerId,
                        ClubId = matchPerf.Player.ClubId, // Вземаме клуб ID-то от самия играч
                        Appearances = 0,
                        Goals = 0,
                        Assists = 0,
                        YellowCards = 0,
                        RedCards = 0,
                        AverageRating = 0m
                    };

                    // Добавяме го в речника и казваме на Entity Framework да го следи за запис
                    seasonStatsDict[matchPerf.PlayerId] = seasonStat;
                    _context.PlayerSeasonStats.Add(seasonStat);
                }

                // Преизчисляваме средната оценка (AverageRating)
                // Формула: ((Стара Оценка * Стари Участия) + Нова Оценка) / Нови Участия
                decimal totalRatingPoints = seasonStat.AverageRating * seasonStat.Appearances;

                // Добавяме статистиките от текущия мач
                seasonStat.Appearances += 1;
                seasonStat.Goals += matchPerf.Goals;
                seasonStat.Assists += matchPerf.Assists;
                seasonStat.YellowCards += matchPerf.YellowCards;
                seasonStat.RedCards += matchPerf.RedCards;

                // Изчисляваме новата средна оценка и я закръгляме до 2 знака след запетаята
                totalRatingPoints += matchPerf.MatchRating;
                seasonStat.AverageRating = Math.Round(totalRatingPoints / seasonStat.Appearances, 2);
            }
        }
    }
}