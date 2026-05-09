namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using TenPercent.Application.DTOs;
    using TenPercent.Application.DTOs.Scouting;
    using TenPercent.Application.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class ScoutingController : ControllerBase
    {
        private readonly IScoutingService _scoutingService;

        public ScoutingController(IScoutingService scoutingService)
        {
            _scoutingService = scoutingService;
        }

        // POST: api/scouting/request-report
        [HttpPost("request-report")]
        public async Task<IActionResult> RequestReport([FromBody] RequestReportDto dto)
        {
            var result = await _scoutingService.GeneratePaidReportAsync(dto.UserId, dto.PlayerId);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = "Scout report generated successfully!", report = result.Report });
        }

        // GET: api/scouting/report/{userId}/{playerId}
        [HttpGet("report/{userId}/{playerId}")]
        public async Task<IActionResult> GetReport(int userId, int playerId)
        {
            var report = await _scoutingService.GetReportAsync(userId, playerId);

            if (report == null)
            {
                return NotFound(new { message = "Нямате доклад за този играч." });
            }

            return Ok(report);
        }
    }
}