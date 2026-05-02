namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using TenPercent.Application.Interfaces;
    using TenPercent.Application.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class SimulationController : ControllerBase
    {
        private readonly ISimulationService _simulationService;

        public SimulationController(ISimulationService simulationService)
        {
            _simulationService = simulationService;
        }

        // POST: api/simulation/play-gameweek
        [HttpPost("play-gameweek")]
        public async Task<IActionResult> PlayNextGameweek()
        {
            var result = await _simulationService.SimulateNextGameweekAsync();

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
    }
}