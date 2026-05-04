namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using TenPercent.Application.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly IStatsService _statsService;

        public StatsController(IStatsService statsService)
        {
            _statsService = statsService;
        }

        [HttpGet("season")]
        public async Task<IActionResult> GetSeasonStats()
        {
            var stats = await _statsService.GetCurrentSeasonStatsAsync();
            return Ok(stats);
        }
    }
}