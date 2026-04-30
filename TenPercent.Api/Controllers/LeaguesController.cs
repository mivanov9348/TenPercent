namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TenPercent.Data;

    [Route("api/[controller]")]
    [ApiController]
    public class LeaguesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LeaguesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("standings")]
        public async Task<IActionResult> GetStandings()
        {
            // Взимаме лигите, отборите и САМО изиграните мачове
            var leagues = await _context.Leagues
                .Include(l => l.Clubs)
                .Include(l => l.Fixtures.Where(f => f.IsPlayed))
                .ToListAsync();

            var result = leagues.Select(l => new
            {
                l.Id,
                l.Name,
                // Смятаме класирането за всеки отбор в лигата
                Standings = l.Clubs.Select(c =>
                {
                    var homeMatches = l.Fixtures.Where(f => f.HomeClubId == c.Id).ToList();
                    var awayMatches = l.Fixtures.Where(f => f.AwayClubId == c.Id).ToList();

                    int wins = homeMatches.Count(f => f.HomeGoals > f.AwayGoals) + awayMatches.Count(f => f.AwayGoals > f.HomeGoals);
                    int draws = homeMatches.Count(f => f.HomeGoals == f.AwayGoals) + awayMatches.Count(f => f.AwayGoals == f.HomeGoals);
                    int losses = homeMatches.Count(f => f.HomeGoals < f.AwayGoals) + awayMatches.Count(f => f.AwayGoals < f.HomeGoals);

                    int gf = homeMatches.Sum(f => f.HomeGoals) + awayMatches.Sum(f => f.AwayGoals);
                    int ga = homeMatches.Sum(f => f.AwayGoals) + awayMatches.Sum(f => f.HomeGoals);
                    int gd = gf - ga;

                    return new
                    {
                        ClubId = c.Id,
                        Team = c.Name,
                        P = wins + draws + losses,
                        W = wins,
                        D = draws,
                        L = losses,
                        GD = gd,
                        Pts = (wins * 3) + draws
                    };
                })
                .OrderByDescending(c => c.Pts) // Сортираме по Точки
                .ThenByDescending(c => c.GD)   // После по Голова разлика
                .ThenBy(c => c.Team)           // И накрая по азбучен ред
                .Select((c, index) => new
                {
                    Pos = index + 1, // Позицията в класирането (1, 2, 3...)
                    c.ClubId,
                    c.Team,
                    c.P,
                    c.W,
                    c.D,
                    c.L,
                    GD = c.GD > 0 ? $"+{c.GD}" : c.GD.ToString(), // Форматираме (напр. +5, -2, 0)
                    c.Pts
                })
                .ToList()
            });

            return Ok(result);
        }
    }
}