namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Api.DTOs;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;

    public class StatsService : IStatsService
    {
        private readonly AppDbContext _context;

        public StatsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SeasonStatsDto> GetCurrentSeasonStatsAsync()
        {
            var activeSeason = await _context.Seasons.FirstOrDefaultAsync(s => s.IsActive);

            // Ако няма активен сезон, връщаме празен обект (списоците вътре са вече инициализирани)
            if (activeSeason == null)
            {
                return new SeasonStatsDto();
            }

            var baseQuery = _context.PlayerSeasonStats
                .Include(ps => ps.Player)
                .ThenInclude(p => p.Club)
                .Where(ps => ps.SeasonId == activeSeason.Id && ps.Appearances > 0);

            var topScorers = await baseQuery
                .Where(ps => ps.Goals > 0)
                .OrderByDescending(ps => ps.Goals)
                .ThenBy(ps => ps.Appearances)
                .Take(10)
                .Select(ps => new TopStatDto
                {
                    Id = ps.Player.Id,
                    Name = ps.Player.Name,
                    Club = ps.Player.Club != null ? ps.Player.Club.Name : "Free Agent",
                    Value = (decimal)ps.Goals,
                    Matches = ps.Appearances
                })
                .ToListAsync();

            var topRatings = await baseQuery
                .OrderByDescending(ps => ps.AverageRating)
                .Take(10)
                .Select(ps => new TopStatDto
                {
                    Id = ps.Player.Id,
                    Name = ps.Player.Name,
                    Club = ps.Player.Club != null ? ps.Player.Club.Name : "Free Agent",
                    Value = (decimal)ps.AverageRating,
                    Matches = ps.Appearances
                })
                .ToListAsync();

            var topAssists = await baseQuery
                .Where(ps => ps.Assists > 0)
                .OrderByDescending(ps => ps.Assists)
                .ThenBy(ps => ps.Appearances)
                .Take(10)
                .Select(ps => new TopStatDto
                {
                    Id = ps.Player.Id,
                    Name = ps.Player.Name,
                    Club = ps.Player.Club != null ? ps.Player.Club.Name : "Free Agent",
                    Value = (decimal)ps.Assists,
                    Matches = ps.Appearances
                })
                .ToListAsync();

            var mostCards = await baseQuery
                .Where(ps => ps.YellowCards > 0 || ps.RedCards > 0)
                .OrderByDescending(ps => ps.RedCards * 3 + ps.YellowCards)
                .Take(10)
                .Select(ps => new CardStatDto
                {
                    Id = ps.Player.Id,
                    Name = ps.Player.Name,
                    Club = ps.Player.Club != null ? ps.Player.Club.Name : "Free Agent",
                    Yellow = ps.YellowCards,
                    Red = ps.RedCards,
                    Matches = ps.Appearances
                })
                .ToListAsync();

            return new SeasonStatsDto
            {
                TopScorers = topScorers,
                TopRatings = topRatings,
                TopAssists = topAssists,
                MostCards = mostCards
            };
        }
    }
}