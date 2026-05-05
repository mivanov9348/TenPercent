namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.AspNetCore.Identity; // ДОБАВЕНО ЗА UserManager
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
        private readonly UserManager<IdentityUser> _userManager; // ДОБАВЕНО

        public AuthController(AppDbContext context, IAgencyService agencyService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _agencyService = agencyService;
            _userManager = userManager; // ДОБАВЕНО
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (await _context.GameUsers.AnyAsync(u => u.Email == dto.Email || u.Username == dto.Username))
            {
                return BadRequest(new { message = "Потребител с този имейл или име вече съществува." });
            }

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _context.GameUsers.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { userId = user.Id, message = "Registration successful!" });
        }

        // POST: api/auth/login 
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            // 1. ПЪРВО ПРОВЕРЯВАМЕ ДАЛИ Е НОРМАЛЕН ИГРАЧ (в GameUsers)
            var playerUser = await _context.GameUsers
                .Include(u => u.Agent)
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (playerUser != null)
            {
                if (!BCrypt.Net.BCrypt.Verify(dto.Password, playerUser.PasswordHash))
                    return Unauthorized(new { message = "Грешна парола." });

                return Ok(new
                {
                    userId = playerUser.Id.ToString(), // Връщаме като string
                    hasAgency = playerUser.Agent != null,
                    role = playerUser.Role,
                    message = "Login successful!"
                });
            }

            // 2. АКО НЕ Е ИГРАЧ, ПРОВЕРЯВАМЕ ДАЛИ Е АДМИН (в Identity)
            var adminUser = await _userManager.FindByEmailAsync(dto.Email);

            if (adminUser != null)
            {
                // Identity има собствен начин за проверка на пароли (не ползва нашия BCrypt)
                if (!await _userManager.CheckPasswordAsync(adminUser, dto.Password))
                    return Unauthorized(new { message = "Грешна парола." });

                var roles = await _userManager.GetRolesAsync(adminUser);
                string role = roles.FirstOrDefault() ?? "Admin";

                return Ok(new
                {
                    userId = adminUser.Id, // Identity връща GUID (string)
                    hasAgency = false, // Админът няма агенция
                    role = role,
                    message = "Admin login successful!"
                });
            }

            // 3. АКО ГО НЯМА НИКЪДЕ:
            return Unauthorized(new { message = "Грешен имейл или парола." });
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