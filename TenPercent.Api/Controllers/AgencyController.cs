namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using TenPercent.Api.DTOs;
    using TenPercent.Application.DTOs;
    using TenPercent.Application.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class AgencyController : ControllerBase
    {
        private readonly IAgencyService _agencyService;

        public AgencyController(IAgencyService agencyService)
        {
            _agencyService = agencyService;
        }

        // ==========================================
        // СЪЗДАВАНЕ НА АГЕНЦИЯ
        // ==========================================
        [HttpPost("create")]
        public async Task<IActionResult> CreateAgency([FromBody] CreateAgencyDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Невалидни данни.");

            var result = await _agencyService.CreateAgencyAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        // ==========================================
        // ВЗИМАНЕ НА ДАННИ ЗА АГЕНЦИЯТА
        // ==========================================
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetMyAgency(int userId)
        {
            var agencyDto = await _agencyService.GetMyAgencyAsync(userId);

            if (agencyDto == null)
                return NotFound(new { message = "Agency not found for this user." });

            return Ok(agencyDto);
        }

        // ==========================================
        // ОСТАНАЛИТЕ МЕТОДИ
        // ==========================================
        [HttpGet("{userId}/players")]
        public async Task<IActionResult> GetAgencyPlayers(int userId)
        {
            var players = await _agencyService.GetAgencyPlayersAsync(userId);

            if (players == null)
                return NotFound("Agency not found for this user.");

            return Ok(players);
        }

        [HttpPost("{userId}/offer-contract")]
        public async Task<IActionResult> OfferRepresentation(int userId, [FromBody] OfferRepresentationDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Невалидни данни в офертата.");

            var result = await _agencyService.OfferRepresentationAsync(userId, dto);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            if (!result.Accepted)
            {
                return Ok(new { accepted = false, message = result.Message });
            }

            return Ok(new { accepted = true, message = result.Message });
        }

        [HttpGet("{userId}/finance")]
        public async Task<IActionResult> GetAgencyFinance(int userId)
        {
            var financeData = await _agencyService.GetAgencyFinanceAsync(userId);

            if (financeData == null)
                return NotFound("Agency not found for this user.");

            return Ok(financeData);
        }
    }
}