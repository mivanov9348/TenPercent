// --- Services/PlayerService.cs ---
namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Api.DTOs;
    using TenPercent.Application.DTOs;
    using TenPercent.Application.Interfaces;
    using TenPercent.Data;

    public class PlayerService : IPlayerService
    {
        private readonly AppDbContext _context;
        private readonly IScoutingEngine _scoutingEngine;

        public PlayerService(AppDbContext context, IScoutingEngine scoutingEngine)
        {
            _context = context;
            _scoutingEngine = scoutingEngine;
        }

        public async Task<PlayerDetailsDto> GetPlayerDetailsAsync(int id)
        {

            // 1. Identify the current active season
            var worldState = await _context.WorldStates.FirstOrDefaultAsync();
            var activeSeasonId = worldState?.CurrentSeasonId;

            // 2. Fetch the core player entity with all necessary related data
            var player = await _context.Players
                .Include(p => p.Club)
                .Include(p => p.Agency)
                .Include(p => p.Attributes)
                .Include(p => p.Position)
                .Include(p => p.ClubContracts.Where(c => c.IsActive))
                .Include(p => p.SeasonPerformances.Where(sp => sp.SeasonId == activeSeasonId))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (player == null) return null;

            // 3. Fetch recent match performances separately to avoid cartesian explosion in EF Core
            var recentMatches = await _context.PlayerMatchPerformances
                .Include(m => m.Fixture)
                    .ThenInclude(f => f.HomeClub)
                .Include(m => m.Fixture)
                    .ThenInclude(f => f.AwayClub)
                .Where(m => m.PlayerId == id && m.Fixture.IsPlayed)
                .OrderByDescending(m => m.Fixture.ScheduledDate)
                .Take(5)
                .ToListAsync();

            // Extract the active contract and current season stats (guaranteed single due to filters above)
            var activeContract = player.ClubContracts.FirstOrDefault();
            var currentSeasonStats = player.SeasonPerformances.FirstOrDefault();

            // 4. Map to DTO
            return new PlayerDetailsDto
            {
                Id = player.Id,
                Name = player.Name,
                Age = player.Age,
                Nationality = player.Nationality,
                Position = player.Position?.Abbreviation ?? "UNK",
                OVR = player.CurrentAbility,
                POT = player.PotentialAbility,

                // Core Attributes
                Pace = player.Attributes.Pace,
                Shooting = player.Attributes.Shooting,
                Passing = player.Attributes.Passing,
                Dribbling = player.Attributes.Dribbling,
                Defending = player.Attributes.Defending,
                Physical = player.Attributes.Physical,

                // Hidden/Mental Attributes
                Ambition = player.Attributes.Ambition,
                Greed = player.Attributes.Greed,
                Loyalty = player.Attributes.Loyalty,

                // Market & Affiliations
                MarketValue = player.MarketValue,
                Form = player.Form,
                ClubId = player.ClubId,
                ClubName = player.Club?.Name,
                AgencyName = player.Agency?.Name,
                HasAgent = player.AgencyId != null,

                // Contract Details
                WeeklyWage = activeContract?.WeeklyWage ?? 0,
                ContractYearsLeft = activeContract != null ? (activeContract.EndSeasonNumber - activeContract.StartSeasonNumber) : 0,

                // Season Performance Stats
                SeasonAppearances = currentSeasonStats?.Appearances ?? 0,
                SeasonGoals = currentSeasonStats?.Goals ?? 0,
                SeasonAssists = currentSeasonStats?.Assists ?? 0,
                SeasonAverageRating = currentSeasonStats?.AverageRating ?? 0,
                SeasonYellowCards = currentSeasonStats?.YellowCards ?? 0,
                SeasonRedCards = currentSeasonStats?.RedCards ?? 0,

                // Match Log
                RecentMatches = recentMatches.Select(m => new PlayerMatchDto
                {
                    Gameweek = m.Fixture.Gameweek,
                    MatchDate = m.Fixture.ScheduledDate,
                    OpponentName = m.Fixture.HomeClubId == player.ClubId ? m.Fixture.AwayClub.Name : m.Fixture.HomeClub.Name,
                    IsHomeMatch = m.Fixture.HomeClubId == player.ClubId,
                    MinutesPlayed = m.MinutesPlayed,
                    Goals = m.Goals,
                    Assists = m.Assists,
                    Rating = Math.Round(m.MatchRating, 1)
                }).ToList()
            };
        }

        public async Task<PaginatedResultDto<ScoutingPlayerDto>> GetScoutingPoolAsync(
            string? search, string? position, string? nationality,
            int? minAge, int? maxAge, decimal? maxValue,
            bool? hasAgency, string? sortBy, int page, int pageSize)
        {
            // Намираме текущия сезон, за да покажем актуалните статистики
            var worldState = await _context.WorldStates.FirstOrDefaultAsync();
            var activeSeasonId = worldState?.CurrentSeasonId ?? 0;

            var query = _context.Players
                .Include(p => p.Club)
                .Include(p => p.Position)
                .Include(p => p.Attributes)
                .Include(p => p.ClubContracts)
                // НОВО: Взимаме само статистиките за текущия сезон
                .Include(p => p.SeasonPerformances.Where(sp => sp.SeasonId == activeSeasonId))
                .AsQueryable();

            // Apply filters dynamically
            if (hasAgency.HasValue) query = hasAgency.Value ? query.Where(p => p.AgencyId != null) : query.Where(p => p.AgencyId == null);
            if (!string.IsNullOrWhiteSpace(search)) query = query.Where(p => p.Name.Contains(search));
            if (!string.IsNullOrWhiteSpace(position) && position != "All") query = query.Where(p => p.Position.Abbreviation == position);
            if (!string.IsNullOrWhiteSpace(nationality)) query = query.Where(p => p.Nationality.Contains(nationality));
            if (minAge.HasValue) query = query.Where(p => p.Age >= minAge.Value);
            if (maxAge.HasValue) query = query.Where(p => p.Age <= maxAge.Value);
            if (maxValue.HasValue) query = query.Where(p => p.MarketValue <= maxValue.Value);

            // Сортиране
            query = sortBy switch
            {
                "Value" => query.OrderByDescending(p => p.MarketValue),
                "ValueAsc" => query.OrderBy(p => p.MarketValue),
                "Age" => query.OrderBy(p => p.Age),
                "AgeDesc" => query.OrderByDescending(p => p.Age),
                "Wage" => query.OrderByDescending(p => p.ClubContracts.Any(c => c.IsActive)
                                ? p.ClubContracts.FirstOrDefault(c => c.IsActive)!.WeeklyWage : 0),
                "Pace" => query.OrderByDescending(p => p.Attributes.Pace),
                "Shooting" => query.OrderByDescending(p => p.Attributes.Shooting),
                "Passing" => query.OrderByDescending(p => p.Attributes.Passing),
                "Dribbling" => query.OrderByDescending(p => p.Attributes.Dribbling),
                "Defending" => query.OrderByDescending(p => p.Attributes.Defending),
                "Physical" => query.OrderByDescending(p => p.Attributes.Physical),

                // НОВО: Сортиране по статистика
                "Goals" => query.OrderByDescending(p => p.SeasonPerformances.FirstOrDefault() != null ? p.SeasonPerformances.FirstOrDefault()!.Goals : 0),
                "Assists" => query.OrderByDescending(p => p.SeasonPerformances.FirstOrDefault() != null ? p.SeasonPerformances.FirstOrDefault()!.Assists : 0),
                "Rating" => query.OrderByDescending(p => p.SeasonPerformances.FirstOrDefault() != null ? p.SeasonPerformances.FirstOrDefault()!.AverageRating : 0),

                _ => query.OrderByDescending(p => p.MarketValue)
            };

            var totalCount = await query.CountAsync();

            var rawPlayers = await query
                 .Skip((page - 1) * pageSize)
                 .Take(pageSize)
                 .ToListAsync();

            int currentScoutingLevel = 1; // "Fog of War" ниво (засега хардкоднато)

            var playersDto = rawPlayers.Select(p =>
            {
                var currentSeasonStats = p.SeasonPerformances.FirstOrDefault();

                return new ScoutingPlayerDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Position = p.Position?.Abbreviation ?? "UNK",
                    Age = p.Age,
                    Nationality = p.Nationality,
                    MarketValue = p.MarketValue,
                    ClubName = p.Club != null ? p.Club.Name : "Free Agent",
                    HasAgency = p.AgencyId != null,

                    Pace = _scoutingEngine.MaskAttribute(p.Attributes.Pace, currentScoutingLevel),
                    Shooting = _scoutingEngine.MaskAttribute(p.Attributes.Shooting, currentScoutingLevel),
                    Passing = _scoutingEngine.MaskAttribute(p.Attributes.Passing, currentScoutingLevel),
                    Dribbling = _scoutingEngine.MaskAttribute(p.Attributes.Dribbling, currentScoutingLevel),
                    Defending = _scoutingEngine.MaskAttribute(p.Attributes.Defending, currentScoutingLevel),
                    Physical = _scoutingEngine.MaskAttribute(p.Attributes.Physical, currentScoutingLevel),

                    // --- МАПВАНЕ НА СТАТИСТИКАТА ---
                    Apps = currentSeasonStats?.Appearances ?? 0,
                    Goals = currentSeasonStats?.Goals ?? 0,
                    Assists = currentSeasonStats?.Assists ?? 0,
                    AvgRating = currentSeasonStats?.AverageRating ?? 0m,

                    WeeklyWage = p.ClubContracts.Any(c => c.IsActive)
                                 ? p.ClubContracts.FirstOrDefault(c => c.IsActive)!.WeeklyWage
                                 : 0
                };
            }).ToList();

            return new PaginatedResultDto<ScoutingPlayerDto>
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Items = playersDto
            };
        }
    }
}
