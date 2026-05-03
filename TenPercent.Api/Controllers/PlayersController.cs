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
                .Include(p => p.Position) // ВАЖНО: Трябва да инклуднем позицията
                .FirstOrDefaultAsync(p => p.Id == id);

            if (player == null) return NotFound(new { message = "Player not found." });

            var response = new
            {
                player.Id,
                player.Name,
                player.Age,
                player.Nationality,
                Position = player.Position?.Abbreviation ?? "UNK", // Връщаме стринга (напр. "ST")
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
            // РЕШЕНИЕТО НА ГРЕШКАТА: Дърпаме позициите
            var positions = await _context.Positions.ToListAsync();
            if (!positions.Any())
            {
                return BadRequest(new { message = "Positions not imported. Please import positions first." });
            }

            // Подаваме ги на генератора
            var newPlayers = _playerGenerator.GenerateMultiplePlayers(count, "", null, positions);

            _context.Players.AddRange(newPlayers);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Successfully generated {count} organic free agents into the world!" });
        }

        [HttpGet("get-pool")]
        public async Task<IActionResult> GetScoutingPool(
            [FromQuery] string? search,
            [FromQuery] string? position,
            [FromQuery] string? nationality,
            [FromQuery] int? minAge,
            [FromQuery] int? maxAge,
            [FromQuery] decimal? maxValue,
            [FromQuery] bool? hasAgency,
            [FromQuery] string? sortBy = "Value",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            // Включваме Position, за да можем да филтрираме по него
            var query = _context.Players
                .Include(p => p.Club)
                .Include(p => p.Position)
                .AsQueryable();

            if (hasAgency.HasValue) query = hasAgency.Value ? query.Where(p => p.AgencyId != null) : query.Where(p => p.AgencyId == null);
            if (!string.IsNullOrEmpty(search)) query = query.Where(p => p.Name.Contains(search));

            // Филтриране по позиция (по Abbreviation)
            if (!string.IsNullOrEmpty(position) && position != "All") query = query.Where(p => p.Position.Abbreviation == position);

            if (!string.IsNullOrEmpty(nationality)) query = query.Where(p => p.Nationality.Contains(nationality));
            if (minAge.HasValue) query = query.Where(p => p.Age >= minAge.Value);
            if (maxAge.HasValue) query = query.Where(p => p.Age <= maxAge.Value);
            if (maxValue.HasValue) query = query.Where(p => p.MarketValue <= maxValue.Value);

            query = sortBy switch
            {
                "Value" => query.OrderByDescending(p => p.MarketValue),
                "ValueAsc" => query.OrderBy(p => p.MarketValue),
                "Age" => query.OrderBy(p => p.Age),
                "AgeDesc" => query.OrderByDescending(p => p.Age),
                _ => query.OrderByDescending(p => p.MarketValue)
            };

            var totalCount = await query.CountAsync();

            var players = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new {
                    p.Id,
                    p.Name,
                    Position = p.Position.Abbreviation, // Връщаме стринга за React
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