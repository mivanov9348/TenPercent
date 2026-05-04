namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using TenPercent.Application.Services.Interfaces;

    [Route("api/[controller]")]
    [ApiController]
    public class ClubsController : ControllerBase
    {
        private readonly IClubService _clubService;

        public ClubsController(IClubService clubService)
        {
            _clubService = clubService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetClubDetails(int id)
        {
            var clubDetails = await _clubService.GetClubDetailsAsync(id);

            if (clubDetails == null)
            {
                return NotFound(new { message = "Club not found." });
            }

            return Ok(clubDetails);
        }
    }
}