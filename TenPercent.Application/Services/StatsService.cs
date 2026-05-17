namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
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
            // 1. НАМИРАНЕ НА СЕЗОНА: Първо търсим по флаг IsActive (най-сигурно)
            var activeSeason = await _context.Seasons.FirstOrDefaultAsync(s => s.IsActive);

            // Ако няма активен сезон по флаг, опитваме през WorldState като резервен вариант
            if (activeSeason == null)
            {
                var worldState = await _context.WorldStates.FirstOrDefaultAsync();
                if (worldState != null && worldState.CurrentSeasonId.HasValue)
                {
                    activeSeason = await _context.Seasons.FindAsync(worldState.CurrentSeasonId.Value);
                }
            }

            // Ако наистина няма никакъв сезон, връщаме празни инициализирани масиви, за да не гърми React
            if (activeSeason == null)
            {
                return new SeasonStatsDto
                {
                    TopScorers = new List<TopStatDto>(),
                    TopRatings = new List<TopStatDto>(),
                    TopAssists = new List<TopStatDto>(),
                    MostCards = new List<CardStatDto>()
                };
            }

            // 2. БАЗОВА ЗАЯВКА: МАХНАХМЕ ps.Appearances > 0.
            // По този начин, дори симулаторът да забрави да добави "Appearance" на играча,
            // ако той има гол или асистенция, пак ще излезе в списъка!
            var baseQuery = _context.PlayerSeasonStats // Забележка: Ако в AppDbContext се казва PlayerSeasonStats, промени го!
                .Include(ps => ps.Player)
                .ThenInclude(p => p.Club)
                .Where(ps => ps.SeasonId == activeSeason.Id);

            var topScorers = await baseQuery
                .Where(ps => ps.Goals > 0)
                .OrderByDescending(ps => ps.Goals)
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
                .Where(ps => ps.AverageRating > 0) // Взимаме само играчи с реален рейтинг
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
                .OrderByDescending(ps => (ps.RedCards * 3) + ps.YellowCards) // Червеният картон тежи като 3 жълти за класацията
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