namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;
    using TenPercent.Application.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.Models;
    using TenPercent.Application.DTOs;
    using System.Linq;

    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private readonly IPlayerService _playerService;
        private readonly IPlayerGeneratorService _playerGeneratorService;
        private readonly AppDbContext _context;

        public PlayersController(
            IPlayerService playerService,
            IPlayerGeneratorService playerGeneratorService,
            AppDbContext context)
        {
            _playerService = playerService;
            _playerGeneratorService = playerGeneratorService;
            _context = context;
        }

        // GET: api/players/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlayerDetails(int id)
        {
            var player = await _playerService.GetPlayerDetailsAsync(id);

            if (player == null)
                return NotFound(new { message = $"Player with ID {id} not found." });

            return Ok(player);
        }

        // POST: api/players/generate-free-agents
        [HttpPost("generate-free-agents")]
        public async Task<IActionResult> GenerateFreeAgents([FromQuery] int count = 50)
        {
            var result = await _playerGeneratorService.GenerateFreeAgentsAsync(count);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        // GET: api/players/get-pool
        [HttpGet("get-pool")]
        public async Task<IActionResult> GetScoutingPool(
            [FromQuery] string? search,
            [FromQuery] string? position,
            [FromQuery] string? nationality,
            [FromQuery] int? minAge,
            [FromQuery] int? maxAge,
            [FromQuery] decimal? maxValue,
            [FromQuery] bool? hasAgency,
            [FromQuery] string? AgencyName,
            [FromQuery] string? sortBy = "Value",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var pool = await _playerService.GetScoutingPoolAsync(
                search, position, nationality, minAge, maxAge,
                maxValue, hasAgency, AgencyName, sortBy, page, pageSize);

            return Ok(pool);
        }

        // --- ДОБАВЯНЕ В ШОРТЛИСТ ---
        [HttpPost("{playerId}/shortlist")]
        public async Task<IActionResult> AddToShortlist(int playerId, [FromBody] AddToShortlistDto dto)
        {
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .FirstOrDefaultAsync(a => a.UserId == dto.UserId);

            if (agent?.Agency == null)
                return BadRequest(new { message = "You do not have an agency." });

            var alreadyShortlisted = await _context.AgencyShortlists
                .AnyAsync(s => s.AgencyId == agent.Agency.Id && s.PlayerId == playerId);

            if (alreadyShortlisted)
                return BadRequest(new { message = "Player is already in your shortlist." });

            var shortlistEntry = new AgencyShortlist
            {
                AgencyId = agent.Agency.Id,
                PlayerId = playerId,
                AddedAt = System.DateTime.UtcNow
            };

            _context.AgencyShortlists.Add(shortlistEntry);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Player added to shortlist successfully!" });
        }

        // --- ВЗИМАНЕ НА ШОРТЛИСТА ---
        [HttpGet("shortlist/{userId}")]
        public async Task<IActionResult> GetMyShortlist(int userId)
        {
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent?.Agency == null)
                return BadRequest(new { message = "You do not have an agency." });

            // ОПТИМИЗИРАНО: Махнати са излишните Include, защото ползваме Select
            var shortlist = await _context.AgencyShortlists
                .Where(s => s.AgencyId == agent.Agency.Id)
                .OrderByDescending(s => s.AddedAt)
                .Select(s => new
                {
                    s.PlayerId,
                    s.Player.Name,
                    s.Player.Age,
                    // Защита от null референция, ако играчът няма позиция
                    Position = s.Player.Position != null ? s.Player.Position.Abbreviation : "Unknown",
                    s.Player.Nationality,
                    s.Player.MarketValue,
                    ClubName = s.Player.Club != null ? s.Player.Club.Name : "Free Agent",
                    s.AddedAt
                })
                .ToListAsync();

            return Ok(shortlist);
        }

        // --- ПРЕМАХВАНЕ ОТ ШОРТЛИСТА ---
        [HttpDelete("{playerId}/shortlist/{userId}")]
        public async Task<IActionResult> RemoveFromShortlist(int playerId, int userId)
        {
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent?.Agency == null)
                return BadRequest(new { message = "You do not have an agency." });

            var entry = await _context.AgencyShortlists
                .FirstOrDefaultAsync(s => s.AgencyId == agent.Agency.Id && s.PlayerId == playerId);

            if (entry == null)
                return NotFound(new { message = "Player is not in your shortlist." });

            _context.AgencyShortlists.Remove(entry);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Player removed from shortlist." });
        }
    }
}