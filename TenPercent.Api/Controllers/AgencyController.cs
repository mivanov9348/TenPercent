namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using TenPercent.Application.DTOs;
    using TenPercent.Application.Services.Interfaces;


    [Route("api/[controller]")]
    [ApiController]
    public class AgencyController : ControllerBase
    {
        private readonly IAgencyService _agencyService;

        public AgencyController(IAgencyService agencyService)
        {
            // Вече инжектираме САМО сървиза!
            _agencyService = agencyService;
        }

        // GET: api/agency/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetMyAgency(int userId)
        {
            var agencyDto = await _agencyService.GetMyAgencyAsync(userId);

            if (agencyDto == null)
                return NotFound("Agency not found for this user.");

            return Ok(agencyDto);
        }

        // GET: api/agency/{userId}/players
        [HttpGet("{userId}/players")]
        public async Task<IActionResult> GetAgencyPlayers(int userId)
        {
            var players = await _agencyService.GetAgencyPlayersAsync(userId);

            if (players == null)
                return NotFound("Agency not found for this user.");

            return Ok(players);
        }

        // POST: api/agency/{userId}/offer-contract
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
    }
}