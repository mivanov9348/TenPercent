namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
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

        public SimulationService(AppDbContext context, IMatchEngineService matchEngine)
        {
            _context = context;
            _matchEngine = matchEngine;
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

            // 1. Взимаме всички мачове за текущия Gameweek
            var fixturesToPlay = await _context.Fixtures
                .Include(f => f.HomeClub).ThenInclude(c => c.Players).ThenInclude(p => p.Attributes)
                .Include(f => f.AwayClub).ThenInclude(c => c.Players).ThenInclude(p => p.Attributes)
                .Where(f => f.SeasonId == activeSeason.Id && f.Gameweek == activeSeason.CurrentGameweek && !f.IsPlayed)
                .ToListAsync();

            if (!fixturesToPlay.Any())
                return (false, $"Няма намерени мачове за изиграване в Gameweek {activeSeason.CurrentGameweek}.");

            // 2. Взимаме живото класиране за ъпдейт
            var standings = await _context.LeagueStandings.ToListAsync();

            // 3. Симулираме всеки мач
            foreach (var match in fixturesToPlay)
            {
                var playedMatch = await _matchEngine.PlayMatchAsync(match);

                // 4. Ъпдейтваме класирането за този мач
                UpdateStandings(standings, playedMatch);
            }

            // 5. Увеличаваме Gameweek на Сезона с 1 (Минаваме на следващия ден)
            activeSeason.CurrentGameweek++;

            // 6. Обновяваме датата на следващия мач във WorldState (Ако е свързана с таймера във фронтенда)
            worldState.NextMatchdayDate = DateTime.UtcNow.AddDays(1);

            await _context.SaveChangesAsync();

            return (true, $"Gameweek {activeSeason.CurrentGameweek - 1} симулиран успешно! Изиграни {fixturesToPlay.Count} мача.");
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
    }
}