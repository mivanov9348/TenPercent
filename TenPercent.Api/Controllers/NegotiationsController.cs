namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using TenPercent.Application.DTOs;
    using TenPercent.Application.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class NegotiationsController : ControllerBase
    {
        private readonly INegotiationService _negotiationService;

        public NegotiationsController(INegotiationService negotiationService)
        {
            _negotiationService = negotiationService;
        }

        [HttpPost("propose")]
        public async Task<IActionResult> ProposeContract([FromBody] ContractOfferDto dto, [FromQuery] int userId)
        {
            if (userId <= 0) return Unauthorized("Invalid user.");
            var response = await _negotiationService.ProposeContractAsync(userId, dto);
            if (response.Status == "Error") return BadRequest(new { message = response.Message });
            return Ok(response);
        }

        [HttpPost("renew")]
        public async Task<IActionResult> RenewContract([FromBody] ContractOfferDto dto, [FromQuery] int userId)
        {
            if (userId <= 0) return Unauthorized("Invalid user.");
            var response = await _negotiationService.RenewContractAsync(userId, dto);
            if (response.Status == "Error") return BadRequest(new { message = response.Message });
            return Ok(response);
        }


        [HttpPost("approach-agent")]
        public async Task<IActionResult> ApproachAgent([FromBody] AgencyPoachOfferDto dto, [FromQuery] int userId)
        {
            if (userId <= 0) return Unauthorized("Invalid user.");
            var result = await _negotiationService.SendPoachOfferAsync(userId, dto);

            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(new { message = result.Message });
        }

        [HttpPost("respond-approach")]
        public async Task<IActionResult> RespondToApproach([FromBody] RespondToMessageOfferDto dto, [FromQuery] int userId)
        {
            if (userId <= 0) return Unauthorized("Invalid user.");
            var result = await _negotiationService.RespondToPoachOfferAsync(userId, dto);

            if (!result.Success) return BadRequest(new { message = result.Message });
            return Ok(new { message = result.Message });
        }
    }
}