namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TenPercent.Api.DTOs;
    using TenPercent.Data;

    [Route("api/[controller]")]
    [ApiController]
    public class AgencyController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AgencyController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/agency/{userId}
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetMyAgency(int userId)
        {
            // Намираме агента и неговата агенция чрез UserId
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .ThenInclude(ag => ag.Players) // Включваме играчите, за да ги преброим
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent == null || agent.Agency == null)
            {
                return NotFound("Agency not found for this user.");
            }

            // Мапваме към DTO
            var agencyDto = new AgencyDto
            {
                Id = agent.Agency.Id,
                Name = agent.Agency.Name,
                AgentName = agent.Name, // Взимаме името от Агента
                LogoId = agent.Agency.LogoId,
                Budget = agent.Agency.Budget,
                Reputation = agent.Agency.Reputation,
                Level = agent.Agency.Level,
                EstablishedAt = agent.Agency.EstablishedAt,
                TotalPlayersCount = agent.Agency.Players.Count // Брой играчи
            };

            return Ok(agencyDto);
        }
    }
}