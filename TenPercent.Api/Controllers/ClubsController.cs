namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TenPercent.Data;

    [Route("api/[controller]")]
    [ApiController]
    public class ClubsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClubsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClubDetails(int id)
        {
            var club = await _context.Clubs
                .Include(c => c.League)
                .Include(c => c.Players)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (club == null) return NotFound(new { message = "Club not found." });

            // МАГИЯТА: Използваме CurrentAbility и PotentialAbility от базата, 
            // но ги кръщаваме Overall и Potential за фронтенда, за да не чупим React таблиците.
            var cleanPlayers = club.Players.Select(p => new
            {
                p.Id,
                p.Name,
                p.Age,
                p.Position,
                Overall = p.CurrentAbility,     // ПОПРАВЕНО
                Potential = p.PotentialAbility, // ПОПРАВЕНО
                p.MarketValue
            }).ToList();

            var response = new
            {
                club.Id,
                club.Name,
                club.Country,
                club.City,
                LeagueName = club.League?.Name ?? "Unknown",
                club.PrimaryColor,
                club.Reputation,
                club.TransferBudget,
                club.WageBudget,
                Squad = new
                {
                    Goalkeepers = cleanPlayers.Where(p => p.Position.Abbreviation == "GK").OrderByDescending(p => p.Overall).ToList(),
                    Defenders = cleanPlayers.Where(p => p.Position.Abbreviation == "DEF").OrderByDescending(p => p.Overall).ToList(),
                    Midfielders = cleanPlayers.Where(p => p.Position.Abbreviation == "MID").OrderByDescending(p => p.Overall).ToList(),
                    Strikers = cleanPlayers.Where(p => p.Position.Abbreviation == "ST").OrderByDescending(p => p.Overall).ToList(),
                }
            };

            return Ok(response);
        }
    }
}