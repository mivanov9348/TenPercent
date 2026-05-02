namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using TenPercent.Application.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class LeaguesController : ControllerBase
    {
        private readonly ILeagueService _leagueService;

        public LeaguesController(ILeagueService leagueService)
        {
            _leagueService = leagueService;
        }

        [HttpGet("standings")]
        public async Task<IActionResult> GetStandings()
        {
            var result = await _leagueService.GetLiveStandingsAsync();
            return Ok(result);
        }
    }
}