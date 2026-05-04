namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TenPercent.Api.DTOs;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.Models;

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAgencyService _agencyService; 

        public AuthController(AppDbContext context, IAgencyService agencyService)
        {
            _context = context;
            _agencyService = agencyService;
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

        [HttpPost("create-agency")]
        public async Task<IActionResult> CreateAgency(CreateAgencyDto dto)
        {
            var result = await _agencyService.CreateAgencyAsync(dto);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
    }
}