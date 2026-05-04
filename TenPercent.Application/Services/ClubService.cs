namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Api.DTOs;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;

    public class ClubService : IClubService
    {
        private readonly AppDbContext _context;

        public ClubService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ClubDetailsDto> GetClubDetailsAsync(int id)
        {
            var club = await _context.Clubs
                .Include(c => c.League)
                .Include(c => c.Players)
                    .ThenInclude(p => p.Position) // ПОПРАВКА: Трябва ни за да вземем Abbreviation!
                .FirstOrDefaultAsync(c => c.Id == id);

            if (club == null) return null;

            // Мапваме играчите към новото DTO
            var cleanPlayers = club.Players.Select(p => new ClubPlayerDto
            {
                Id = p.Id,
                Name = p.Name,
                Age = p.Age,
                Position = p.Position?.Abbreviation ?? "Unknown", // Предпазваме се от евентуален null
                Overall = p.CurrentAbility,
                Potential = p.PotentialAbility,
                MarketValue = p.MarketValue
            }).ToList();

            // Връщаме готовия, подреден обект
            return new ClubDetailsDto
            {
                Id = club.Id,
                Name = club.Name,
                Country = club.Country,
                City = club.City,
                LeagueName = club.League?.Name ?? "Unknown",
                PrimaryColor = club.PrimaryColor,
                Reputation = club.Reputation,
                TransferBudget = club.TransferBudget,
                WageBudget = club.WageBudget,
                Squad = new ClubSquadDto
                {
                    Goalkeepers = cleanPlayers.Where(p => p.Position == "GK").OrderByDescending(p => p.Overall).ToList(),
                    Defenders = cleanPlayers.Where(p => p.Position == "DEF").OrderByDescending(p => p.Overall).ToList(),
                    Midfielders = cleanPlayers.Where(p => p.Position == "MID").OrderByDescending(p => p.Overall).ToList(),
                    Strikers = cleanPlayers.Where(p => p.Position == "ST").OrderByDescending(p => p.Overall).ToList(),
                }
            };
        }
    }
}