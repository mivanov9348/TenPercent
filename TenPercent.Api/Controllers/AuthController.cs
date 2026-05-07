namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Data;
    using TenPercent.Data.Models;

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        // ВЕЧЕ ПОЛЗВАМЕ НАШИЯ КЛАС User !
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _context;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // 1. Създаваме нашия User
            var user = new User
            {
                UserName = dto.Username,
                Email = dto.Email,
                Role = "Player",
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Player");

                // Връщаме int ID на React-а
                return Ok(new { userId = user.Id, message = "Registration successful" });
            }

            return BadRequest(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // 1. Търсим по имейл
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Unauthorized("Грешен имейл или парола.");

            // 2. Проверяваме паролата
            var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded) return Unauthorized("Грешен имейл или парола.");

            // 3. Проверяваме дали този User вече си е създал Агенция
            bool hasAgency = await _context.Agents
                .Include(a => a.Agency)
                .AnyAsync(a => a.UserId == user.Id && a.Agency != null);

            // 4. Връщаме данните към React
            return Ok(new
            {
                userId = user.Id,
                hasAgency = hasAgency,
                role = user.Role
            });
        }
    }

    // Помощни DTO класове (ако ги държиш в отделна папка, можеш да ги изтриеш от тук и да си ги import-неш)
    public class RegisterDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}