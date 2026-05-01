namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using TenPercent.Application.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class SeasonController : ControllerBase
    {
        private readonly ISeasonService _seasonService;

        public SeasonController(ISeasonService seasonService)
        {
            _seasonService = seasonService;
        }

        [HttpPost("end")]
        public async Task<IActionResult> EndSeason()
        {
            var result = await _seasonService.EndCurrentSeasonAsync();
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(new { message = result.Message });
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartSeason()
        {
            var result = await _seasonService.StartNewSeasonAsync();
            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(new { message = result.Message });
        }
    }
}