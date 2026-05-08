namespace TenPercent.Api.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using TenPercent.Data;

    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // ПРОМЯНА: Добавяме userId като параметър (може да е nullable, ако админ гледа дашборда)
        [HttpGet("home/{userId?}")]
        public async Task<IActionResult> GetHomeDashboard(int? userId)
        {
            var worldState = await _context.WorldStates.FirstOrDefaultAsync();

            if (worldState == null)
            {
                return Ok(new { IsInitialized = false });
            }

            var activeSeason = await _context.Seasons.FirstOrDefaultAsync(s => s.Id == worldState.CurrentSeasonId);

            var upcomingFixtures = new List<object>();
            var previousFixtures = new List<object>();
            var clientMatches = new List<object>();
            var clientReports = new List<object>();

            if (activeSeason != null)
            {
                // 1. ГЛОБАЛНИ ПРЕДСТОЯЩИ МАЧОВЕ (За всички)
                upcomingFixtures = await _context.Fixtures
                    .Include(f => f.HomeClub)
                    .Include(f => f.AwayClub)
                    .Include(f => f.League)
                    .Where(f => f.SeasonId == activeSeason.Id && f.Gameweek == activeSeason.CurrentGameweek && !f.IsPlayed)
                    .OrderBy(f => f.League.Name).ThenBy(f => f.HomeClub.Name)
                    .Take(20) // Ограничаваме до 20, за да не гърми фронтенда
                    .Select(f => new
                    {
                        Id = f.Id,
                        HomeTeam = f.HomeClub.Name,
                        AwayTeam = f.AwayClub.Name,
                        League = f.League.Name,
                        Date = f.ScheduledDate
                    })
                    .Cast<object>()
                    .ToListAsync();

                // 2. ГЛОБАЛНИ ИЗИГРАНИ МАЧОВЕ (За всички)
                if (activeSeason.CurrentGameweek > 1)
                {
                    previousFixtures = await _context.Fixtures
                        .Include(f => f.HomeClub)
                        .Include(f => f.AwayClub)
                        .Include(f => f.League)
                        .Where(f => f.SeasonId == activeSeason.Id && f.Gameweek == activeSeason.CurrentGameweek - 1 && f.IsPlayed)
                        .OrderBy(f => f.League.Name).ThenBy(f => f.HomeClub.Name)
                        .Take(20)
                        .Select(f => new
                        {
                            Id = f.Id,
                            HomeTeam = f.HomeClub.Name,
                            HomeGoals = f.HomeGoals,
                            AwayTeam = f.AwayClub.Name,
                            AwayGoals = f.AwayGoals,
                            League = f.League.Name,
                            Date = f.ScheduledDate
                        })
                        .Cast<object>()
                        .ToListAsync();
                }

                // 3. ПЕРСОНАЛНИ ДАННИ ЗА АГЕНТА (Ако има подаден userId)
                if (userId.HasValue)
                {
                    var agent = await _context.Agents
                        .Include(a => a.Agency)
                        .FirstOrDefaultAsync(a => a.UserId == userId.Value);

                    if (agent?.Agency != null)
                    {
                        int agencyId = agent.Agency.Id;

                        // Взимаме ID-тата на всички клубове, в които агенцията има клиенти
                        var clientClubIds = await _context.Players
                            .Where(p => p.AgencyId == agencyId && p.ClubId.HasValue)
                            .Select(p => p.ClubId.Value)
                            .Distinct()
                            .ToListAsync();

                        // 3.1. MY CLIENT MATCHES (Предстоящи мачове на клубовете на клиентите)
                        if (clientClubIds.Any())
                        {
                            // 1. Взимаме самите мачове
                            var rawClientMatches = await _context.Fixtures
                                .Include(f => f.HomeClub)
                                .Include(f => f.AwayClub)
                                .Include(f => f.League)
                                .Where(f => f.SeasonId == activeSeason.Id
                                         && f.Gameweek == activeSeason.CurrentGameweek
                                         && !f.IsPlayed
                                         && (clientClubIds.Contains(f.HomeClubId) || clientClubIds.Contains(f.AwayClubId)))
                                .OrderBy(f => f.ScheduledDate)
                                .ToListAsync();

                            // 2. Взимаме клиентите на тази агенция (за да знаем кой къде играе)
                            var agencyClients = await _context.Players
                                .Include(p => p.Position)
                                .Where(p => p.AgencyId == agencyId && p.ClubId.HasValue)
                                .ToListAsync();

                            // 3. Сглобяваме ги в паметта
                            clientMatches = rawClientMatches.Select(f => new
                            {
                                Id = f.Id,
                                HomeTeam = f.HomeClub.Name,
                                AwayTeam = f.AwayClub.Name,
                                League = f.League.Name,
                                Date = f.ScheduledDate,

                                // НОВО: Намираме кои клиенти са в домакина ИЛИ госта за този мач
                                PlayersInvolved = agencyClients
                                    .Where(p => p.ClubId == f.HomeClubId || p.ClubId == f.AwayClubId)
                                    .Select(p => new { Name = p.Name, Pos = p.Position.Abbreviation })
                                    .ToList()
                            })
                            .Cast<object>()
                            .ToList();
                        }

                        // 3.2. CLIENT REPORTS (Оценки от изиграни мачове на клиентите, сортирани от най-новите)
                        clientReports = await _context.PlayerMatchPerformances
                            .Include(pmp => pmp.Player)
                            .ThenInclude(p => p.Position)
                            .Include(pmp => pmp.Fixture)
                            .Where(pmp => pmp.Player.AgencyId == agencyId && pmp.MinutesPlayed > 0)
                            .OrderByDescending(pmp => pmp.Fixture.Gameweek) // Най-новите мачове първи
                            .ThenByDescending(pmp => pmp.MatchRating) // След това по оценка
                            .Take(30)
                            .Select(pmp => new
                            {
                                Id = pmp.Id,
                                PlayerName = pmp.Player.Name,
                                Position = pmp.Player.Position.Abbreviation,
                                Rating = pmp.MatchRating,
                                Goals = pmp.Goals,
                                Assists = pmp.Assists,
                                Minutes = pmp.MinutesPlayed,
                                Gameweek = pmp.Fixture.Gameweek,
                                Result = pmp.Fixture.HomeClubId == pmp.Player.ClubId
                                    ? $"{pmp.Fixture.HomeGoals} - {pmp.Fixture.AwayGoals}"
                                    : $"{pmp.Fixture.AwayGoals} - {pmp.Fixture.HomeGoals}"
                            })
                            .Cast<object>()
                            .ToListAsync();
                    }
                }
            }

            var topPlayers = await _context.Players
                .Include(p => p.Club)
                .Include(p => p.Position)
                .OrderByDescending(p => p.CurrentAbility)
                .Take(5)
                .Select(p => new
                {
                    Id = p.Id,
                    Name = p.Name,
                    Club = p.Club != null ? p.Club.Name : "Free Agent",
                    OVR = p.CurrentAbility,
                    Position = p.Position != null ? p.Position.Abbreviation : "UNK"
                })
                .ToListAsync();

            return Ok(new
            {
                IsInitialized = true,
                WorldState = new
                {
                    CurrentSeasonId = worldState.CurrentSeasonId,
                    SeasonNumber = activeSeason?.SeasonNumber ?? 0,
                    CurrentGameweek = activeSeason?.CurrentGameweek ?? 0,
                    TotalGameweeks = activeSeason?.TotalGameweeks ?? 0,
                    IsSeasonActive = activeSeason != null && activeSeason.IsActive,
                    NextMatchdayDate = worldState.NextMatchdayDate
                },
                UpcomingMatches = upcomingFixtures,
                PreviousMatches = previousFixtures,
                TopPlayers = topPlayers,
                ClientMatches = clientMatches,
                ClientReports = clientReports
            });
        }

        [HttpGet("top-scorers")]
        public async Task<IActionResult> GetTopScorers()
        {
            var activeSeason = await _context.Seasons.FirstOrDefaultAsync(s => s.IsActive);
            if (activeSeason == null) return Ok(new List<object>());

            var topScorers = await _context.PlayerMatchPerformances
                .Include(pmp => pmp.Player)
                .ThenInclude(p => p.Club)
                .Where(pmp => pmp.Fixture.SeasonId == activeSeason.Id && pmp.Goals > 0)
                .GroupBy(pmp => new { pmp.Player.Id, pmp.Player.Name, ClubName = pmp.Player.Club != null ? pmp.Player.Club.Name : "Free Agent" })
                .Select(g => new
                {
                    Id = g.Key.Id,
                    Name = g.Key.Name,
                    Team = g.Key.ClubName,
                    Goals = g.Sum(x => x.Goals),
                    Matches = g.Count()
                })
                .OrderByDescending(g => g.Goals)
                .ThenBy(g => g.Matches)
                .Take(50)
                .ToListAsync();

            var result = topScorers.Select((s, index) => new
            {
                Rank = index + 1,
                s.Id,
                s.Name,
                s.Team,
                s.Goals,
                s.Matches
            });

            return Ok(result);
        }
    }
}