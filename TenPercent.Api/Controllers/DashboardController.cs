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

            // 1. Предстоящи мачове (Взимаме 5 мача от текущия кръг)
            var upcomingFixtures = new List<object>();
            if (activeSeason != null)
            {
                upcomingFixtures = await _context.Fixtures
                    .Include(f => f.HomeClub)
                    .Include(f => f.AwayClub)
                    .Include(f => f.League)
                    .Where(f => f.SeasonId == activeSeason.Id && !f.IsPlayed && f.Gameweek >= activeSeason.CurrentGameweek)
                    .OrderBy(f => f.ScheduledDate)
                    .Take(5)
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
            }

            // 2. Топ играчи (Взимаме ги по CurrentAbility засега, по-късно ще е по MatchRating)
            var topPlayers = await _context.Players
                .Include(p => p.Club)
                .OrderByDescending(p => p.CurrentAbility)
                .Take(5)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Club = p.Club != null ? p.Club.Name : "Free Agent",
                    OVR = p.CurrentAbility,
                    Position = p.Position
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
                    //NextMatchdayDate = worldState.NextMatchdayDate
                },
                UpcomingMatches = upcomingFixtures,
                TopPlayers = topPlayers,
                // Засега връщаме празни масиви за клиентските специфики, докато не направим логиката за Агенцията
                ClientMatches = new List<object>(),
                ClientReports = new List<object>()
            });
        }
    }
}