namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;
    public class LeagueService : ILeagueService
    {
        private readonly AppDbContext _context;

        public LeagueService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<object> GetLiveStandingsAsync()
        {
            // Дърпаме всички "Живи" класирания (LeagueStandings), заедно с имената на клубовете и лигите
            var standings = await _context.LeagueStandings
                .Include(ls => ls.Club)
                .Include(ls => ls.League)
                .ToListAsync();

            // Групираме ги по Лига
            var leaguesGrouped = standings.GroupBy(ls => ls.League);

            var result = leaguesGrouped.Select(group => new
            {
                Id = group.Key.Id,
                Name = group.Key.Name,

                Standings = group
                    // Сортираме директно по готовите полета в базата
                    .OrderByDescending(c => c.Points)
                    .ThenByDescending(c => (c.GoalsFor - c.GoalsAgainst)) // Голова разлика
                    .ThenBy(c => c.Club.Name)
                    .Select((c, index) => new
                    {
                        Pos = index + 1,
                        ClubId = c.ClubId,
                        Team = c.Club.Name,
                        P = c.Played,
                        W = c.Won,
                        D = c.Drawn,
                        L = c.Lost,
                        // Смятаме голова разлика и форматираме с + ако е положителна
                        GD = (c.GoalsFor - c.GoalsAgainst) > 0 ? $"+{c.GoalsFor - c.GoalsAgainst}" : (c.GoalsFor - c.GoalsAgainst).ToString(),
                        Pts = c.Points
                    })
                    .ToList()
            }).ToList();

            return result;
        }
    }
}