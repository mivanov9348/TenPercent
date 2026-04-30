namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TenPercent.Api.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.Models;

    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPlayerGeneratorService _playerGen;

        public AdminController(AppDbContext context, IPlayerGeneratorService playerGen)
        {
            _context = context;
            _playerGen = playerGen;
        }

        // ==========================================
        // СТЪПКА 1: Създаване на света (Отбори и Играчи)
        // ==========================================
        [HttpPost("init-world")]
        public async Task<IActionResult> InitializeWorld()
        {
            // Проверка дали светът вече е създаден
            if (await _context.WorldStates.AnyAsync())
                return BadRequest("The world is already initialized! Delete the database if you want to start over.");

            // 1. Създаване на WorldState
            var world = new WorldState
            {
                CurrentSeason = 1,
                CurrentGameweek = 1,
                TotalGameweeks = 18, // 10 отбора по 2 мача (домакин/гост) = 18 кръга
                NextMatchdayDate = DateTime.UtcNow.AddDays(2), // Първият мач е след 2 дни
                IsSimulationRunning = false
            };
            _context.WorldStates.Add(world);

            // 2. Създаване на Лигата
            var league = new League { Name = "English Premier Division", Country = "England", Reputation = 90 };
            _context.Leagues.Add(league);
            await _context.SaveChangesAsync(); // Запазваме, за да вземем LeagueId

            // 3. Създаване на 10 Отбора (Засега ги хардкодваме за лесно тестване)
            var clubNames = new[] { "Man Red", "London Blue", "London Cannons", "Merseyside Red", "Man Blue",
                                    "North London White", "Aston Lions", "Newcastle Magpies", "West Ham", "Brighton" };

            var clubs = new List<Club>();
            foreach (var name in clubNames)
            {
                clubs.Add(new Club
                {
                    Name = name,
                    Country = "England",
                    LeagueId = league.Id,
                    Reputation = new Random().Next(70, 95),
                    TransferBudget = 50000000m,
                    WageBudget = 2000000m
                });
            }
            _context.Clubs.AddRange(clubs);
            await _context.SaveChangesAsync(); // Запазваме, за да вземем ClubIds

            // 4. Генериране на играчи за отборите (по 15 на отбор)
            var allPlayers = new List<Player>();
            foreach (var club in clubs)
            {
                allPlayers.AddRange(_playerGen.GenerateMultiplePlayers(15, "Normal", club.Id));
            }

            // 5. Генериране на 50 Свободни агента (Смес от нормални и wonderkids)
            allPlayers.AddRange(_playerGen.GenerateMultiplePlayers(40, "Normal", null));
            allPlayers.AddRange(_playerGen.GenerateMultiplePlayers(10, "Wonderkid", null));

            _context.Players.AddRange(allPlayers);
            await _context.SaveChangesAsync();

            return Ok(new { message = "World initialized successfully! League, 10 Clubs, and Players created." });
        }

        // ==========================================
        // СТЪПКА 2: Генериране на мачовете (Round-Robin)
        // ==========================================
        [HttpPost("generate-schedule")]
        public async Task<IActionResult> GenerateSchedule()
        {
            var league = await _context.Leagues.Include(l => l.Clubs).FirstOrDefaultAsync();
            if (league == null) return BadRequest("No league found. Run init-world first.");

            if (await _context.Fixtures.AnyAsync(f => f.LeagueId == league.Id))
                return BadRequest("Schedule is already generated.");

            var clubs = league.Clubs.ToList();
            var fixtures = new List<Fixture>();

            int numClubs = clubs.Count;
            int numRounds = numClubs - 1;
            int matchesPerRound = numClubs / 2;

            // Класически Round-Robin алгоритъм за първия полусезон
            for (int round = 0; round < numRounds; round++)
            {
                for (int match = 0; match < matchesPerRound; match++)
                {
                    int home = (round + match) % (numClubs - 1);
                    int away = (numClubs - 1 - match + round) % (numClubs - 1);

                    if (match == 0) away = numClubs - 1; // Последният отбор стои статичен

                    fixtures.Add(new Fixture
                    {
                        LeagueId = league.Id,
                        Season = 1,
                        Gameweek = round + 1, // Кръгове от 1 до 9
                        HomeClubId = clubs[home].Id,
                        AwayClubId = clubs[away].Id,
                        ScheduledDate = DateTime.UtcNow.AddDays((round + 1) * 3) // Всеки мач е през 3 дни
                    });
                }
            }

            // Огледално копиране за втория полусезон (Разменяме Домакин и Гост)
            var secondHalfFixtures = new List<Fixture>();
            foreach (var fix in fixtures)
            {
                secondHalfFixtures.Add(new Fixture
                {
                    LeagueId = league.Id,
                    Season = 1,
                    Gameweek = fix.Gameweek + numRounds, // Кръгове от 10 до 18
                    HomeClubId = fix.AwayClubId, // Гостът става Домакин
                    AwayClubId = fix.HomeClubId, // Домакинът става Гост
                    ScheduledDate = DateTime.UtcNow.AddDays((fix.Gameweek + numRounds) * 3)
                });
            }

            _context.Fixtures.AddRange(fixtures);
            _context.Fixtures.AddRange(secondHalfFixtures);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Successfully generated {fixtures.Count + secondHalfFixtures.Count} fixtures for {numRounds * 2} Gameweeks." });
        }
    }
}