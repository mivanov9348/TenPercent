using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TenPercent.Api.DTOs;
using TenPercent.Data;
using TenPercent.Data.Models;
using BCrypt.Net;

namespace TenPercent.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            // 1. Проверка дали потребителят съществува
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email || u.Username == dto.Username))
            {
                return BadRequest("User with this email or username already exists.");
            }

            // 2. Създаване на нов User и хеширане на паролата
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Връщаме ID-то, за да може фронтендът да продължи към създаване на агенция
            return Ok(new { userId = user.Id, message = "Registration successful!" });
        }

        // POST: api/auth/login 
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _context.Users
                .Include(u => u.Agent)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid email or password.");
            }

            bool hasAgency = user.Agent != null;

            return Ok(new
            {
                userId = user.Id,
                hasAgency = hasAgency,
                role = user.Role, 
                message = "Login successful!"
            });
        }

        // POST: api/auth/create-agency
        [HttpPost("create-agency")]
        public async Task<IActionResult> CreateAgency(CreateAgencyDto dto)
        {
            // 1. Намираме потребителя
            var user = await _context.Users.Include(u => u.Agent).FirstOrDefaultAsync(u => u.Id == dto.UserId);
            if (user == null) return NotFound("User not found.");

            if (user.Agent != null) return BadRequest("This user already has an agency.");

            // 2. Създаваме Агента
            var agent = new Agent
            {
                Name = dto.AgentName,
                UserId = user.Id
            };

            // 3. Създаваме Агенцията и я връзваме с Агента
            var agency = new Agency
            {
                Name = dto.AgencyName,
                LogoId = dto.LogoId,
                Agent = agent // EF Core автоматично ще свърже ID-тата!
            };

            _context.Agents.Add(agent);
            _context.Agencies.Add(agency);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Agency created successfully! Welcome to the game." });
        }
    }
}