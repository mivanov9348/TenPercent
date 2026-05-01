namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TenPercent.Application.Interfaces;
    using TenPercent.Data;

    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPlayerGeneratorService _playerGenerator;

        public PlayersController(AppDbContext context, IPlayerGeneratorService playerGenerator)
        {
            _context = context;
            _playerGenerator = playerGenerator;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlayerDetails(int id)
        {
            var player = await _context.Players
                .Include(p => p.Club)
                .Include(p => p.Agency)
                .Include(p => p.Attributes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (player == null) return NotFound(new { message = "Player not found." });

            var response = new
            {
                player.Id,
                player.Name,
                player.Age,
                player.Nationality,
                player.Position,
                OVR = player.CurrentAbility,
                POT = player.PotentialAbility,

                Pace = player.Attributes.Pace,
                Shooting = player.Attributes.Shooting,
                Passing = player.Attributes.Passing,
                Dribbling = player.Attributes.Dribbling,
                Defending = player.Attributes.Defending,
                Physical = player.Attributes.Physical,

                Ambition = player.Attributes.Ambition,
                Greed = player.Attributes.Greed,
                Loyalty = player.Attributes.Loyalty,

                player.MarketValue,
                player.WeeklyWage,
                player.ContractYearsLeft,
                player.Form,
                ClubId = player.ClubId,
                ClubName = player.Club?.Name,
                AgencyName = player.Agency?.Name
            };

            return Ok(response);
        }

        [HttpPost("generate-free-agents")]
        public async Task<IActionResult> GenerateFreeAgents([FromQuery] int count = 50)
        {
            var newPlayers = _playerGenerator.GenerateMultiplePlayers(count, "", null);
            _context.Players.AddRange(newPlayers);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Successfully generated {count} organic free agents into the world!" });
        }

        // GET: api/players/get-pool
        [HttpGet("get-pool")]
        public async Task<IActionResult> GetScoutingPool(
            [FromQuery] string? search,
            [FromQuery] string? position,
            [FromQuery] string? nationality,
            [FromQuery] int? minAge,             // НОВО: Минимална възраст
            [FromQuery] int? maxAge,
            [FromQuery] decimal? maxValue,
            [FromQuery] bool? hasAgency,
            [FromQuery] string? sortBy = "Value",  // Променено Default сортиране
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _context.Players
                .Include(p => p.Club)
                .AsQueryable();

            // Основно филтриране
            if (hasAgency.HasValue) query = hasAgency.Value ? query.Where(p => p.AgencyId != null) : query.Where(p => p.AgencyId == null);
            if (!string.IsNullOrEmpty(search)) query = query.Where(p => p.Name.Contains(search));
            if (!string.IsNullOrEmpty(position) && position != "All") query = query.Where(p => p.Position == position);
            if (!string.IsNullOrEmpty(nationality)) query = query.Where(p => p.Nationality.Contains(nationality));

            // Числови филтри
            if (minAge.HasValue) query = query.Where(p => p.Age >= minAge.Value);
            if (maxAge.HasValue) query = query.Where(p => p.Age <= maxAge.Value);
            if (maxValue.HasValue) query = query.Where(p => p.MarketValue <= maxValue.Value);

            // Сортиране (Премахнати са OVR и POT)
            query = sortBy switch
            {
                "Value" => query.OrderByDescending(p => p.MarketValue),
                "ValueAsc" => query.OrderBy(p => p.MarketValue),
                "Age" => query.OrderBy(p => p.Age),
                "AgeDesc" => query.OrderByDescending(p => p.Age),
                _ => query.OrderByDescending(p => p.MarketValue) // Default
            };

            var totalCount = await query.CountAsync();

            var players = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new {
                    p.Id,
                    p.Name,
                    p.Position,
                    p.Age,
                    p.Nationality,
                    p.MarketValue,
                    ClubName = p.Club != null ? p.Club.Name : "Free Agent",
                    HasAgency = p.AgencyId != null
                })
                .ToListAsync();

            return Ok(new
            {
                totalCount,
                page,
                pageSize,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                players
            });
        }
    }
}