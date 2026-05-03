namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Data;

    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StatsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("season")]
        public async Task<IActionResult> GetSeasonStats()
        {
            var activeSeason = await _context.Seasons.FirstOrDefaultAsync(s => s.IsActive);

            // Ако няма активен сезон, връщаме празни масиви
            if (activeSeason == null)
                return Ok(new
                {
                    topScorers = new object[0],
                    topRatings = new object[0],
                    topAssists = new object[0],
                    mostCards = new object[0]
                });

            var baseQuery = _context.PlayerSeasonStats
                .Include(ps => ps.Player)
                .ThenInclude(p => p.Club)
                .Where(ps => ps.SeasonId == activeSeason.Id && ps.Appearances > 0);

            // 1. Топ Голмайстори (Golden Boot)
            var topScorers = await baseQuery
                .Where(ps => ps.Goals > 0)
                .OrderByDescending(ps => ps.Goals)
                .ThenBy(ps => ps.Appearances) // При равни голове, този с по-малко мачове печели
                .Take(10) // Взимаме топ 10
                .Select(ps => new {
                    ps.Player.Id,
                    ps.Player.Name,
                    Club = ps.Player.Club != null ? ps.Player.Club.Name : "Free Agent",
                    Value = ps.Goals,
                    Matches = ps.Appearances
                })
                .ToListAsync();

            // 2. Топ Рейтинг (MVP Race)
            var topRatings = await baseQuery
                .OrderByDescending(ps => ps.AverageRating)
                .Take(10)
                .Select(ps => new {
                    ps.Player.Id,
                    ps.Player.Name,
                    Club = ps.Player.Club != null ? ps.Player.Club.Name : "Free Agent",
                    Value = ps.AverageRating,
                    Matches = ps.Appearances
                })
                .ToListAsync();

            // 3. Топ Асистенти (Playmakers)
            var topAssists = await baseQuery
                .Where(ps => ps.Assists > 0)
                .OrderByDescending(ps => ps.Assists)
                .ThenBy(ps => ps.Appearances)
                .Take(10)
                .Select(ps => new {
                    ps.Player.Id,
                    ps.Player.Name,
                    Club = ps.Player.Club != null ? ps.Player.Club.Name : "Free Agent",
                    Value = ps.Assists,
                    Matches = ps.Appearances
                })
                .ToListAsync();

            // 4. Най-груби играчи (Картони)
            var mostCards = await baseQuery
                .Where(ps => ps.YellowCards > 0 || ps.RedCards > 0)
                .OrderByDescending(ps => ps.RedCards * 3 + ps.YellowCards) // Червеният картон тежи 3 пъти повече
                .Take(10)
                .Select(ps => new {
                    ps.Player.Id,
                    ps.Player.Name,
                    Club = ps.Player.Club != null ? ps.Player.Club.Name : "Free Agent",
                    Yellow = ps.YellowCards,
                    Red = ps.RedCards,
                    Matches = ps.Appearances
                })
                .ToListAsync();

            return Ok(new
            {
                TopScorers = topScorers,
                TopRatings = topRatings,
                TopAssists = topAssists,
                MostCards = mostCards
            });
        }
    }
}