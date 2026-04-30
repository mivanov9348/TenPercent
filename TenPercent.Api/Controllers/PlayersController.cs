namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TenPercent.Api.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.Models;

    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPlayerGeneratorService _playerGenerator;

        // Инжектираме базата данни и интерфейса на нашия сървис
        public PlayersController(AppDbContext context, IPlayerGeneratorService playerGenerator)
        {
            _context = context;
            _playerGenerator = playerGenerator;
        }

        // POST: api/players/generate-wonderkids
        [HttpPost("generate-wonderkids")]
        public async Task<IActionResult> GenerateWonderkids([FromQuery] int count = 10)
        {
            // Използваме сървиса да генерира списъка
            var newPlayers = _playerGenerator.GenerateMultiplePlayers(count, "Wonderkid", null);

            _context.Players.AddRange(newPlayers);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Successfully generated {count} new wonderkids on the free agency market!",
                players = newPlayers
            });
        }

        // GET: api/players/free-agents
        [HttpGet("free-agents")]
        public async Task<IActionResult> GetFreeAgents()
        {
            // Връщаме играчи, които нямат клуб и нямат агент
            var freeAgents = await _context.Players
                .Where(p => p.ClubId == null && p.AgencyId == null)
                .OrderByDescending(p => p.Potential)
                .Take(50) // Връщаме топ 50
                .ToListAsync();

            return Ok(freeAgents);
        }
    }
}