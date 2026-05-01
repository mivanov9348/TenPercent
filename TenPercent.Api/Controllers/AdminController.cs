namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TenPercent.Api.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.Models;
    using CsvHelper;
    using CsvHelper.Configuration;
    using System.Globalization;
    using TenPercent.Api.DTOs;

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

        [HttpPost("import-leagues")]
        public async Task<IActionResult> ImportLeagues(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Please upload a valid leagues.csv file.");

            // FIX 1: Автоматично разпознаване на стила (запетая или точка и запетая)
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                DetectDelimiter = true, // <-- МАГИЯТА: Само намира разделителя!
                ShouldSkipRecord = args => args.Row.Parser.Record.All(string.IsNullOrWhiteSpace)
            };

            using var stream = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(stream, config);

            // FIX 2: Задължително прочитане на заглавния ред преди цикъла!
            csv.Read();
            csv.ReadHeader();

            var records = new List<LeagueImportDto>();
            while (csv.Read())
            {
                var name = csv.GetField<string>("Name");
                if (string.IsNullOrWhiteSpace(name)) continue;

                records.Add(new LeagueImportDto
                {
                    Name = name,
                    Country = csv.GetField<string>("Country") ?? "",
                    Reputation = csv.GetField<int>("Reputation")
                });
            }

            var leagues = records.Select(r => new League
            {
                Name = r.Name,
                Country = r.Country,
                Reputation = r.Reputation
            }).ToList();

            _context.Leagues.AddRange(leagues);

            if (!await _context.WorldStates.AnyAsync())
            {
                _context.WorldStates.Add(new WorldState
                {
                    CurrentSeason = 1,
                    CurrentGameweek = 1,
                    TotalGameweeks = 18,
                    NextMatchdayDate = DateTime.UtcNow.AddDays(2),
                    IsSimulationRunning = false
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Successfully imported {leagues.Count} leagues and initialized world state." });
        }

        [HttpPost("import-clubs")]
        public async Task<IActionResult> ImportClubs(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Please upload a valid clubs.csv file.");

            var existingLeagues = await _context.Leagues.ToListAsync();
            if (!existingLeagues.Any())
                return BadRequest("Please import leagues first!");

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                DetectDelimiter = true,
                ShouldSkipRecord = args => args.Row.Parser.Record.All(string.IsNullOrWhiteSpace)
            };

            using var stream = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(stream, config);

            csv.Read();
            csv.ReadHeader();

            var records = new List<ClubImportDto>();
            while (csv.Read())
            {
                var name = csv.GetField<string>("Name");
                if (string.IsNullOrWhiteSpace(name)) continue;

                // ЧЕТЕМ ГЪВКАВО: Опитваме се да вземем LeagueName, ако го няма - взимаме празен стринг
                csv.TryGetField<string>("LeagueName", out var leagueName);

                // Опитваме се да вземем LeagueId, ако го няма - слагаме 0
                csv.TryGetField<int>("LeagueId", out var leagueId);

                records.Add(new ClubImportDto
                {
                    Name = name,
                    Country = csv.GetField<string>("Country") ?? "",
                    City = csv.GetField<string>("City") ?? "",
                    LeagueName = leagueName ?? "",
                    LeagueId = leagueId,
                    PrimaryColor = csv.GetField<string>("PrimaryColor") ?? "",
                    Reputation = csv.GetField<int>("Reputation"),
                    Level = csv.GetField<int>("Level"),
                    TransferBudget = csv.GetField<decimal>("TransferBudget"),
                    WageBudget = csv.GetField<decimal>("WageBudget")
                });
            }

            var clubs = new List<Club>();

            foreach (var r in records)
            {
                int finalLeagueId = 0;

                // 1. Ако в CSV-то има зададено LeagueId (напр. 1, 2, 3), директно ползваме него
                if (r.LeagueId > 0)
                {
                    // Проверяваме дали такова ID наистина съществува в базата
                    if (!existingLeagues.Any(l => l.Id == r.LeagueId))
                    {
                        return BadRequest($"Error: League with ID {r.LeagueId} does not exist in the database!");
                    }
                    finalLeagueId = r.LeagueId;
                }
                // 2. Ако няма ID, но има Име на лига (напр. "English Premier Division")
                else if (!string.IsNullOrWhiteSpace(r.LeagueName))
                {
                    var matchedLeague = existingLeagues.FirstOrDefault(l =>
                        l.Name.Trim().ToLower() == r.LeagueName.Trim().ToLower());

                    if (matchedLeague == null)
                    {
                        return BadRequest($"Error: League '{r.LeagueName}' not found in the database for club '{r.Name}'.");
                    }
                    finalLeagueId = matchedLeague.Id;
                }
                else
                {
                    return BadRequest($"Error: Club '{r.Name}' must have either LeagueId or LeagueName specified in the CSV.");
                }

                clubs.Add(new Club
                {
                    Name = r.Name,
                    Country = r.Country,
                    City = r.City,
                    LeagueId = finalLeagueId, 
                    PrimaryColor = r.PrimaryColor,
                    Reputation = r.Reputation,
                    Level = r.Level,
                    TransferBudget = r.TransferBudget,
                    WageBudget = r.WageBudget
                });
            }

            _context.Clubs.AddRange(clubs);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Successfully imported {clubs.Count} clubs. Players were NOT generated." });
        }

        [HttpPost("generate-schedule")]
        public async Task<IActionResult> GenerateSchedule()
        {
            var league = await _context.Leagues.Include(l => l.Clubs).FirstOrDefaultAsync();
            if (league == null) return BadRequest("No league found. Run import first.");

            if (await _context.Fixtures.AnyAsync(f => f.LeagueId == league.Id))
                return BadRequest("Schedule is already generated.");

            var clubs = league.Clubs.ToList();
            var fixtures = new List<Fixture>();

            int numClubs = clubs.Count;
            int numRounds = numClubs - 1;
            int matchesPerRound = numClubs / 2;

            for (int round = 0; round < numRounds; round++)
            {
                for (int match = 0; match < matchesPerRound; match++)
                {
                    int home = (round + match) % (numClubs - 1);
                    int away = (numClubs - 1 - match + round) % (numClubs - 1);

                    if (match == 0) away = numClubs - 1;

                    fixtures.Add(new Fixture
                    {
                        LeagueId = league.Id,
                        Season = 1,
                        Gameweek = round + 1,
                        HomeClubId = clubs[home].Id,
                        AwayClubId = clubs[away].Id,
                        ScheduledDate = DateTime.UtcNow.AddDays((round + 1) * 3)
                    });
                }
            }

            var secondHalfFixtures = new List<Fixture>();
            foreach (var fix in fixtures)
            {
                secondHalfFixtures.Add(new Fixture
                {
                    LeagueId = league.Id,
                    Season = 1,
                    Gameweek = fix.Gameweek + numRounds,
                    HomeClubId = fix.AwayClubId,
                    AwayClubId = fix.HomeClubId,
                    ScheduledDate = DateTime.UtcNow.AddDays((fix.Gameweek + numRounds) * 3)
                });
            }

            _context.Fixtures.AddRange(fixtures);
            _context.Fixtures.AddRange(secondHalfFixtures);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Successfully generated {fixtures.Count + secondHalfFixtures.Count} fixtures." });
        }

        [HttpGet("squad-report")]
        public async Task<IActionResult> GetSquadReport()
        {
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
                    report.Add(new
                    {
                        ClubId = club.Id,
                        ClubName = club.Name,
                        CurrentSquadSize = total,
                        Missing = missingPositions
                    });
                }
            }

            if (!report.Any())
                return Ok(new { message = "Всички отбори са в перфектно състояние и готови за мач!" });

            return Ok(new { message = "Открити са проблеми в съставите.", issues = report });
        }


        [HttpPost("squad-autofix")]
        public async Task<IActionResult> AutoFixSquads()
        {
            var clubs = await _context.Clubs.Include(c => c.Players).ToListAsync();
            var newPlayers = new List<Player>();
            int fixedClubsCount = 0;

            foreach (var club in clubs)
            {
                // Ако отборът е напълно празен (напр. току-що импортиран от CSV)
                if (club.Players.Count == 0)
                {
                    // Генерираме му перфектния състав от 16 души чрез новия метод
                    var fullSquad = _playerGen.GenerateFullSquadForClub(club.Id, club.Reputation);
                    newPlayers.AddRange(fullSquad);
                    fixedClubsCount++;
                }
                else
                {
                    // Логика за "закърпване", ако липсват отделни играчи през следващите сезони
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
                        // Резервите ги правим микс
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