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
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .ThenInclude(ag => ag.Players)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent == null || agent.Agency == null)
            {
                return NotFound("Agency not found for this user.");
            }

            var agencyDto = new AgencyDto
            {
                Id = agent.Agency.Id,
                Name = agent.Agency.Name,
                AgentName = agent.Name,
                LogoId = agent.Agency.LogoId,
                Budget = agent.Agency.Budget,
                Reputation = agent.Agency.Reputation,
                Level = agent.Agency.Level,
                EstablishedAt = agent.Agency.EstablishedAt,
                TotalPlayersCount = agent.Agency.Players.Count
            };

            return Ok(agencyDto);
        }

        // --- НОВ МЕТОД: Взима играчите на Агенцията ---
        // GET: api/agency/{userId}/players
        [HttpGet("{userId}/players")]
        public async Task<IActionResult> GetAgencyPlayers(int userId)
        {
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent == null || agent.Agency == null)
                return NotFound("Agency not found.");

            var players = await _context.Players
                .Where(p => p.AgencyId == agent.Agency.Id)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Pos = p.Position,
                    Age = p.Age,
                    Skill = p.CurrentAbility,
                    Potential = p.PotentialAbility,
                    Value = p.MarketValue,
                    Wage = p.WeeklyWage,
                    Contract = p.ContractYearsLeft,
                    Form = p.Form ?? "Average" // Защита, ако Form е null
                })
                .ToListAsync();

            return Ok(players);
        }
    }
}