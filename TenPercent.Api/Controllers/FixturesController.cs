namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using TenPercent.Application.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class FixturesController : ControllerBase
    {
        private readonly IFixtureService _fixtureService;

        public FixturesController(IFixtureService fixtureService)
        {
            _fixtureService = fixtureService;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllFixtures()
        {
            var result = await _fixtureService.GetFixturesByLeagueAndGameweekAsync();

            if (result == null)
                return NotFound(new { message = "No active season or fixtures found." });

            return Ok(result);
        }
    }
}