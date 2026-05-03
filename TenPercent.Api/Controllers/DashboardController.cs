namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TenPercent.Data;

    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("home")]
        public async Task<IActionResult> GetHomeDashboard()
        {
            var worldState = await _context.WorldStates.FirstOrDefaultAsync();

            if (worldState == null)
            {
                return Ok(new { IsInitialized = false });
            }

            var activeSeason = await _context.Seasons.FirstOrDefaultAsync(s => s.Id == worldState.CurrentSeasonId);

            var upcomingFixtures = new List<object>();
            var previousFixtures = new List<object>();

            if (activeSeason != null)
            {
                upcomingFixtures = await _context.Fixtures
                    .Include(f => f.HomeClub)
                    .Include(f => f.AwayClub)
                    .Include(f => f.League)
                    .Where(f => f.SeasonId == activeSeason.Id && f.Gameweek == activeSeason.CurrentGameweek)
                    .OrderBy(f => f.League.Name).ThenBy(f => f.HomeClub.Name)
                    .Select(f => new
                    {
                        Id = f.Id,
                        HomeTeam = f.HomeClub.Name,
                        AwayTeam = f.AwayClub.Name,
                        League = f.League.Name,
                        Date = f.ScheduledDate
                    })
                    .Cast<object>()
                    .ToListAsync();

                if (activeSeason.CurrentGameweek > 1)
                {
                    previousFixtures = await _context.Fixtures
                        .Include(f => f.HomeClub)
                        .Include(f => f.AwayClub)
                        .Include(f => f.League)
                        .Where(f => f.SeasonId == activeSeason.Id && f.Gameweek == activeSeason.CurrentGameweek - 1 && f.IsPlayed)
                        .OrderBy(f => f.League.Name).ThenBy(f => f.HomeClub.Name)
                        .Select(f => new
                        {
                            Id = f.Id,
                            HomeTeam = f.HomeClub.Name,
                            HomeGoals = f.HomeGoals,
                            AwayTeam = f.AwayClub.Name,
                            AwayGoals = f.AwayGoals,
                            League = f.League.Name,
                            Date = f.ScheduledDate
                        })
                        .Cast<object>()
                        .ToListAsync();
                }
            }

            // CRITICAL FIX: Ensure Position is selected as a string (Abbreviation)
            var topPlayers = await _context.Players
                .Include(p => p.Club)
                .Include(p => p.Position) // Ensure Position is included
                .OrderByDescending(p => p.CurrentAbility)
                .Take(5)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Club = p.Club != null ? p.Club.Name : "Free Agent",
                    OVR = p.CurrentAbility,
                    Position = p.Position != null ? p.Position.Abbreviation : "UNK" // Changed this line
                })
                .ToListAsync();

            return Ok(new
            {
                IsInitialized = true,
                WorldState = new
                {
                    CurrentSeasonId = worldState.CurrentSeasonId,
                    SeasonNumber = activeSeason?.SeasonNumber ?? 0,
                    CurrentGameweek = activeSeason?.CurrentGameweek ?? 0,
                    TotalGameweeks = activeSeason?.TotalGameweeks ?? 0,
                    IsSeasonActive = activeSeason != null && activeSeason.IsActive,
                    NextMatchdayDate = worldState.NextMatchdayDate
                },
                UpcomingMatches = upcomingFixtures,
                PreviousMatches = previousFixtures,
                TopPlayers = topPlayers,
                ClientMatches = new List<object>(),
                ClientReports = new List<object>()
            });
        }

        [HttpGet("top-scorers")]
        public async Task<IActionResult> GetTopScorers()
        {
            var activeSeason = await _context.Seasons.FirstOrDefaultAsync(s => s.IsActive);
            if (activeSeason == null) return Ok(new List<object>());

            var topScorers = await _context.PlayerMatchPerformances
                .Include(pmp => pmp.Player)
                .ThenInclude(p => p.Club)
                .Where(pmp => pmp.Fixture.SeasonId == activeSeason.Id && pmp.Goals > 0)
                .GroupBy(pmp => new { pmp.Player.Id, pmp.Player.Name, ClubName = pmp.Player.Club != null ? pmp.Player.Club.Name : "Free Agent" })
                .Select(g => new
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    Team = g.Key.ClubName,
                    Goals = g.Sum(x => x.Goals),
                    Matches = g.Count()
                })
                .OrderByDescending(g => g.Goals)
                .ThenBy(g => g.Matches)
                .Take(50)
                .ToListAsync();

            var result = topScorers.Select((s, index) => new
            {
                Rank = index + 1,
                s.Id,
                s.Name,
                s.Team,
                s.Goals,
                s.Matches
            });

            return Ok(result);
        }
    }
}