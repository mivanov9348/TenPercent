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

            if (response.Status == "Error")
            {
                return BadRequest(new { message = response.Message });
            }

            return Ok(response);
        }
    }
}