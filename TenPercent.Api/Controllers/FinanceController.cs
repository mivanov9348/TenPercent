namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using TenPercent.Application.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class FinanceController : ControllerBase
    {
        private readonly IFinanceService _financeService;

        public FinanceController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        // POST: api/finance/initialize?bankBudget=100000000000
        [HttpPost("initialize")]
        public async Task<IActionResult> InitializeEconomy([FromQuery] decimal bankBudget = 100000000000m)
        {
            if (bankBudget <= 0)
            {
                return BadRequest(new { message = "Бюджетът на банката трябва да е положителен." });
            }

            var result = await _financeService.InitializeWorldEconomyAsync(bankBudget);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
    }
}