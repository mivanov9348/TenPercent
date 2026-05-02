namespace TenPercent.Api.Controllers
{
    using CsvHelper;
    using CsvHelper.Configuration;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Globalization;
    using TenPercent.Api.DTOs;
    using TenPercent.Application.Interfaces;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.Models;

    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPlayerGeneratorService _playerGen;
        private readonly IScheduleService _scheduleService;

        public AdminController(AppDbContext context, IPlayerGeneratorService playerGen, IScheduleService scheduleService)
        {
            _context = context;
            _playerGen = playerGen;
            _scheduleService = scheduleService;
        }

        [HttpPost("initialize-world")]
        public async Task<IActionResult> InitializeWorld()
        {
            if (await _context.WorldStates.AnyAsync())
            {
                return Ok(new { message = "World Engine is already initialized and running." }); // Променено на Ok
            }

            var worldState = new WorldState
            {
                CurrentSeasonId = null,
                IsSimulationRunning = false
            };

            _context.WorldStates.Add(worldState);
            await _context.SaveChangesAsync();

            return Ok(new { message = "World Engine Initialized Successfully! You can now import leagues and clubs." });
        }

        [HttpGet("world-state")]
        public async Task<IActionResult> GetWorldState()
        {
            var worldState = await _context.WorldStates.FirstOrDefaultAsync();

            if (worldState == null)
            {
                return NotFound(new { message = "World Engine is offline." });
            }

            var activeSeason = await _context.Seasons.FirstOrDefaultAsync(s => s.Id == worldState.CurrentSeasonId);

            return Ok(new
            {
                CurrentSeasonId = worldState.CurrentSeasonId,
                SeasonNumber = activeSeason?.SeasonNumber ?? 0,
                CurrentGameweek = activeSeason?.CurrentGameweek ?? 0,
                TotalGameweeks = activeSeason?.TotalGameweeks ?? 0,
                IsSeasonActive = activeSeason != null && activeSeason.IsActive,
                IsSimulationRunning = worldState.IsSimulationRunning,
            });
        }

        [HttpPost("import-leagues")]
        public async Task<IActionResult> ImportLeagues(IFormFile file)
        {
            if (!await _context.WorldStates.AnyAsync()) return BadRequest("World Engine must be initialized first!");
            if (file == null || file.Length == 0) return BadRequest("Please upload a valid leagues.csv file.");

            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true, DetectDelimiter = true, ShouldSkipRecord = args => args.Row.Parser.Record.All(string.IsNullOrWhiteSpace) };
            using var stream = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(stream, config);
            csv.Read(); csv.ReadHeader();

            var leagues = new List<League>();
            while (csv.Read())
            {
                var name = csv.GetField<string>("Name");
                if (!string.IsNullOrWhiteSpace(name))
                    leagues.Add(new League { Name = name, Country = csv.GetField<string>("Country") ?? "", Reputation = csv.GetField<int>("Reputation") });
            }

            _context.Leagues.AddRange(leagues);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Successfully imported {leagues.Count} leagues." });
        }

        [HttpPost("import-clubs")]
        public async Task<IActionResult> ImportClubs(IFormFile file)
        {
            if (!await _context.WorldStates.AnyAsync()) return BadRequest("World Engine must be initialized first!");
            if (file == null || file.Length == 0) return BadRequest("Please upload a valid clubs.csv file.");

            var existingLeagues = await _context.Leagues.ToListAsync();
            if (!existingLeagues.Any()) return BadRequest("Please import leagues first!");

            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true, DetectDelimiter = true, ShouldSkipRecord = args => args.Row.Parser.Record.All(string.IsNullOrWhiteSpace) };
            using var stream = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(stream, config);
            csv.Read(); csv.ReadHeader();

            var clubs = new List<Club>();
            while (csv.Read())
            {
                var name = csv.GetField<string>("Name");
                if (string.IsNullOrWhiteSpace(name)) continue;

                csv.TryGetField<string>("LeagueName", out var leagueName);
                csv.TryGetField<int>("LeagueId", out var leagueId);

                int finalLeagueId = 0;
                if (leagueId > 0)
                {
                    if (!existingLeagues.Any(l => l.Id == leagueId)) return BadRequest($"Error: League with ID {leagueId} does not exist!");
                    finalLeagueId = leagueId;
                }
                else if (!string.IsNullOrWhiteSpace(leagueName))
                {
                    var matchedLeague = existingLeagues.FirstOrDefault(l => l.Name.Trim().ToLower() == leagueName.Trim().ToLower());
                    if (matchedLeague == null) return BadRequest($"Error: League '{leagueName}' not found!");
                    finalLeagueId = matchedLeague.Id;
                }
                else { return BadRequest($"Error: Club '{name}' must have LeagueId or LeagueName."); }

                clubs.Add(new Club
                {
                    Name = name,
                    Country = csv.GetField<string>("Country") ?? "",
                    City = csv.GetField<string>("City") ?? "",
                    LeagueId = finalLeagueId,
                    PrimaryColor = csv.GetField<string>("PrimaryColor") ?? "",
                    Reputation = csv.GetField<int>("Reputation"),
                    Level = csv.GetField<int>("Level"),
                    TransferBudget = csv.GetField<decimal>("TransferBudget"),
                    WageBudget = csv.GetField<decimal>("WageBudget")
                });
            }

            _context.Clubs.AddRange(clubs);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Successfully imported {clubs.Count} clubs. Players were NOT generated." });
        }

        [HttpPost("initialize-standings")]
        public async Task<IActionResult> InitializeStandings()
        {
            var worldState = await _context.WorldStates.FirstOrDefaultAsync();
            if (worldState == null || worldState.CurrentSeasonId == null)
                return BadRequest("World Engine is not running an active season.");

            // ПРОМЯНА: Ако вече има класирания, връщаме ОК със съобщение, че вече са създадени.
            if (await _context.LeagueStandings.AnyAsync())
                return Ok(new { message = "Standings are already initialized for the current season. Ready for fixtures!" });

            var clubs = await _context.Clubs.ToListAsync();
            if (!clubs.Any())
                return BadRequest("No clubs found. Import clubs first.");

            var newStandings = new List<LeagueStanding>();
            foreach (var club in clubs)
            {
                newStandings.Add(new LeagueStanding
                {
                    LeagueId = club.LeagueId,
                    ClubId = club.Id,
                    Played = 0,
                    Won = 0,
                    Drawn = 0,
                    Lost = 0,
                    GoalsFor = 0,
                    GoalsAgainst = 0,
                    Points = 0
                });
            }

            _context.LeagueStandings.AddRange(newStandings);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Successfully initialized standings for {newStandings.Count} clubs. Ready to generate schedule." });
        }

        [HttpPost("generate-schedule")]
        public async Task<IActionResult> GenerateSchedule()
        {
            var worldState = await _context.WorldStates.FirstOrDefaultAsync();
            if (worldState == null || worldState.CurrentSeasonId == null)
                return BadRequest("No active season found to generate schedule for!");

            var activeSeason = await _context.Seasons.FirstOrDefaultAsync(s => s.Id == worldState.CurrentSeasonId);
            if (activeSeason == null || !activeSeason.IsActive)
                return BadRequest("No active season found.");

            if (!await _context.LeagueStandings.AnyAsync())
                return BadRequest("Standings are missing! Please Initialize Standings before generating fixtures.");

            // ПРОМЯНА: Ако вече има мачове, връщаме ОК.
            if (await _context.Fixtures.AnyAsync(f => f.SeasonId == activeSeason.Id))
                return Ok(new { message = "Match Schedule is already generated for the active season. The simulation is ready!" });

            var allLeagues = await _context.Leagues.ToListAsync();
            if (!allLeagues.Any())
                return BadRequest("No leagues found. Run import first.");

            int totalFixturesGenerated = 0;
            var results = new List<string>();

            foreach (var league in allLeagues)
            {
                var result = await _scheduleService.GenerateLeagueScheduleAsync(league.Id, activeSeason.Id);

                if (result.Success)
                {
                    totalFixturesGenerated += result.Fixtures.Count;
                    results.Add(result.Message);
                }
                else
                {
                    results.Add($"Failed for {league.Name}: {result.Message}");
                }
            }

            var maxGameweek = await _context.Fixtures
                .Where(f => f.SeasonId == activeSeason.Id)
                .MaxAsync(f => (int?)f.Gameweek) ?? 0;

            if (maxGameweek > 0)
            {
                activeSeason.TotalGameweeks = maxGameweek;
                _context.Seasons.Update(activeSeason);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = $"Successfully generated {totalFixturesGenerated} fixtures across all leagues. Season set to {maxGameweek} Gameweeks.",
                details = results
            });
        }

        [HttpGet("squad-report")]
        public async Task<IActionResult> GetSquadReport()
        {
            if (!await _context.WorldStates.AnyAsync()) return BadRequest("World Engine must be initialized first!");
            var clubs = await _context.Clubs.Include(c => c.Players).ToListAsync();
            var report = new List<object>();

            foreach (var club in clubs)
            {
                var gks = club.Players.Count(p => p.Position == "GK");
                var defs = club.Players.Count(p => p.Position == "DEF");
                var mids = club.Players.Count(p => p.Position == "MID");
                var sts = club.Players.Count(p => p.Position == "ST");
                var total = club.Players.Count;

                var missingPositions = new List<string>();
                if (gks < 1) missingPositions.Add("GK");
                if (defs < 4) missingPositions.Add("DEF (Needs " + (4 - defs) + ")");
                if (mids < 4) missingPositions.Add("MID (Needs " + (4 - mids) + ")");
                if (sts < 2) missingPositions.Add("ST (Needs " + (2 - sts) + ")");
                if (total < 16) missingPositions.Add($"Total Depth (Needs {16 - total} more)");

                if (missingPositions.Any())
                {
                    report.Add(new { ClubId = club.Id, ClubName = club.Name, CurrentSquadSize = total, Missing = missingPositions });
                }
            }

            if (!report.Any()) return Ok(new { message = "Всички отбори са в перфектно състояние и готови за мач!" });
            return Ok(new { message = "Открити са проблеми в съставите.", issues = report });
        }

        [HttpPost("squad-autofix")]
        public async Task<IActionResult> AutoFixSquads()
        {
            if (!await _context.WorldStates.AnyAsync()) return BadRequest("World Engine must be initialized first!");
            var clubs = await _context.Clubs.Include(c => c.Players).ToListAsync();
            var newPlayers = new List<Player>();
            int fixedClubsCount = 0;

            foreach (var club in clubs)
            {
                if (club.Players.Count == 0)
                {
                    var fullSquad = _playerGen.GenerateFullSquadForClub(club.Id, club.Reputation);
                    newPlayers.AddRange(fullSquad);
                    fixedClubsCount++;
                }
                else
                {
                    bool neededFix = false;
                    string backupTier = "Backup";

                    int gks = club.Players.Count(p => p.Position == "GK");
                    while (gks < 1) { newPlayers.Add(_playerGen.GeneratePlayer("Normal", club.Id, "GK")); gks++; neededFix = true; }

                    int defs = club.Players.Count(p => p.Position == "DEF");
                    while (defs < 4) { newPlayers.Add(_playerGen.GeneratePlayer("Normal", club.Id, "DEF")); defs++; neededFix = true; }

                    int mids = club.Players.Count(p => p.Position == "MID");
                    while (mids < 4) { newPlayers.Add(_playerGen.GeneratePlayer("Normal", club.Id, "MID")); mids++; neededFix = true; }

                    int sts = club.Players.Count(p => p.Position == "ST");
                    while (sts < 2) { newPlayers.Add(_playerGen.GeneratePlayer("Normal", club.Id, "ST")); sts++; neededFix = true; }

                    int currentTotal = club.Players.Count + newPlayers.Count(p => p.ClubId == club.Id);
                    while (currentTotal < 16)
                    {
                        string randomTier = new Random().NextDouble() > 0.5 ? "Prospect" : backupTier;
                        newPlayers.Add(_playerGen.GeneratePlayer(randomTier, club.Id, null));
                        currentTotal++;
                        neededFix = true;
                    }

                    if (neededFix) fixedClubsCount++;
                }
            }

            if (newPlayers.Any())
            {
                _context.Players.AddRange(newPlayers);
                await _context.SaveChangesAsync();
                return Ok(new { message = $"Успешно поправени {fixedClubsCount} отбора. Генерирани {newPlayers.Count} нови играчи." });
            }

            return Ok(new { message = "Няма нужда от поправка. Всички отбори са пълни." });
        }
    }
}