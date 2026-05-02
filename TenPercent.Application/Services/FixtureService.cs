namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;

    public class FixtureService : IFixtureService
    {
        private readonly AppDbContext _context;

        public FixtureService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object?> GetFixturesByLeagueAndGameweekAsync()
        {
            var worldState = await _context.WorldStates.FirstOrDefaultAsync();
            if (worldState == null || worldState.CurrentSeasonId == null) return null;

            var activeSeason = await _context.Seasons.FirstOrDefaultAsync(s => s.Id == worldState.CurrentSeasonId);
            if (activeSeason == null) return null;

            var fixtures = await _context.Fixtures
                .Include(f => f.HomeClub)
                .Include(f => f.AwayClub)
                .Include(f => f.League)
                .Where(f => f.SeasonId == activeSeason.Id)
                .ToListAsync();

            // Групираме: Първо по Лига -> После по Кръг (Gameweek)
            var leagues = fixtures
                .GroupBy(f => new { f.League.Id, f.League.Name })
                .Select(lg => new
                {
                    Id = lg.Key.Id,
                    Name = lg.Key.Name,
                    Gameweeks = lg.GroupBy(f => f.Gameweek)
                                  .OrderBy(gw => gw.Key)
                                  .Select(gw => new
                                  {
                                      Gameweek = gw.Key,
                                      Matches = gw.Select(m => new
                                      {
                                          Id = m.Id,
                                          HomeTeam = m.HomeClub.Name,
                                          HomeGoals = m.HomeGoals,
                                          AwayTeam = m.AwayClub.Name,
                                          AwayGoals = m.AwayGoals,
                                          IsPlayed = m.IsPlayed,
                                          Date = m.ScheduledDate
                                      }).OrderBy(m => m.Date).ToList()
                                  }).ToList()
                }).ToList();

            return new
            {
                CurrentGameweek = activeSeason.CurrentGameweek,
                Leagues = leagues
            };
        }
    }
}