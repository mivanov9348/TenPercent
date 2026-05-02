namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.Models;

    public class ScheduleService : IScheduleService
    {
        private readonly AppDbContext _context;

        public ScheduleService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message, List<Fixture> Fixtures)> GenerateLeagueScheduleAsync(int leagueId, int seasonId)
        {
            var activeSeason = await _context.Seasons.FirstOrDefaultAsync(s => s.Id == seasonId);
            if (activeSeason == null || !activeSeason.IsActive)
                return (false, "Active season not found.", new List<Fixture>());

            bool hasFixtures = await _context.Fixtures
                .AnyAsync(f => f.LeagueId == leagueId && f.SeasonId == seasonId);

            if (hasFixtures)
                return (false, "Schedule is already generated for this league and season.", new List<Fixture>());

            var clubIds = await _context.LeagueStandings
                .Where(ls => ls.LeagueId == leagueId)
                .Select(ls => ls.ClubId)
                .ToListAsync();

            int numClubs = clubIds.Count;

            if (numClubs < 2)
                return (false, $"Not enough clubs initialized in Standings for League ID {leagueId}.", new List<Fixture>());

            bool hasBye = false;
            if (numClubs % 2 != 0)
            {
                clubIds.Add(-1);
                numClubs++;
                hasBye = true;
            }

            int totalRounds = numClubs - 1;
            int matchesPerRound = numClubs / 2;
            var fixtures = new List<Fixture>();
            var currentRoundClubs = new List<int>(clubIds);

            // НОВО: Определяме базовата дата за начало на първенството (напр. Днес)
            // Фиксираме часа на 15:00:00 UTC
            DateTime baseStartDate = DateTime.UtcNow.Date.AddHours(15);

            for (int round = 0; round < totalRounds; round++)
            {
                for (int match = 0; match < matchesPerRound; match++)
                {
                    int homeIndex = match;
                    int awayIndex = numClubs - 1 - match;

                    int homeTeamId = currentRoundClubs[homeIndex];
                    int awayTeamId = currentRoundClubs[awayIndex];

                    if (homeTeamId == -1 || awayTeamId == -1) continue;

                    if (match == 0 && round % 2 == 1)
                    {
                        (homeTeamId, awayTeamId) = (awayTeamId, homeTeamId);
                    }

                    int gameweek = round + 1;

                    // НОВО: Всеки Gameweek е следващият ден от базовата дата
                    var matchDate = baseStartDate.AddDays(gameweek - 1);

                    fixtures.Add(new Fixture
                    {
                        LeagueId = leagueId,
                        SeasonId = seasonId,
                        Gameweek = gameweek,
                        HomeClubId = homeTeamId,
                        AwayClubId = awayTeamId,
                        ScheduledDate = matchDate,
                        IsPlayed = false
                    });
                }

                int lastClub = currentRoundClubs[numClubs - 1];
                for (int i = numClubs - 1; i > 1; i--)
                {
                    currentRoundClubs[i] = currentRoundClubs[i - 1];
                }
                currentRoundClubs[1] = lastClub;
            }

            var secondHalfFixtures = new List<Fixture>();
            foreach (var fix in fixtures)
            {
                secondHalfFixtures.Add(new Fixture
                {
                    LeagueId = leagueId,
                    SeasonId = seasonId,
                    Gameweek = fix.Gameweek + totalRounds,
                    HomeClubId = fix.AwayClubId,
                    AwayClubId = fix.HomeClubId,
                    // Добавяме общия брой кръгове като дни към първоначалната дата на мача
                    ScheduledDate = fix.ScheduledDate.AddDays(totalRounds),
                    IsPlayed = false
                });
            }

            fixtures.AddRange(secondHalfFixtures);
            _context.Fixtures.AddRange(fixtures);
            await _context.SaveChangesAsync();

            return (true, $"Успешно генерирани {fixtures.Count} мача.", fixtures);
        }

        public async Task<(bool Success, string Message, List<Fixture> Fixtures)> GenerateCupScheduleAsync(int seasonId)
        {
            await Task.Delay(10);
            return (false, "Cup schedule generation is not implemented yet.", new List<Fixture>());
        }

        public async Task<(bool Success, string Message, List<Fixture> Fixtures)> GenerateEuropeanScheduleAsync(int seasonId)
        {
            await Task.Delay(10);
            return (false, "European schedule generation is not implemented yet.", new List<Fixture>());
        }
    }
}