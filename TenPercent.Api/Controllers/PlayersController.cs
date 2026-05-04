namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using TenPercent.Application.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private readonly IPlayerService _playerService;

        public PlayersController(IPlayerService playerService)
        {
            _playerService = playerService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlayerDetails(int id)
        {
            var player = await _playerService.GetPlayerDetailsAsync(id);

            if (player == null)
                return NotFound(new { message = "Player not found." });

            return Ok(player);
        }

        [HttpPost("generate-free-agents")]
        public async Task<IActionResult> GenerateFreeAgents([FromQuery] int count = 50)
        {
            var result = await _playerService.GenerateFreeAgentsAsync(count);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
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
            var pool = await _playerService.GetScoutingPoolAsync(
                search, position, nationality, minAge, maxAge,
                maxValue, hasAgency, sortBy, page, pageSize);

            return Ok(pool);
        }
    }
}