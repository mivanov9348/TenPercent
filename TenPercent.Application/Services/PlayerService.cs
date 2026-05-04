namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Api.DTOs;
    using TenPercent.Application.Interfaces;
    using TenPercent.Data;

    public class PlayerService : IPlayerService
    {
        private readonly AppDbContext _context;
        private readonly IPlayerGeneratorService _playerGenerator;

        public PlayerService(AppDbContext context, IPlayerGeneratorService playerGenerator)
        {
            _context = context;
            _playerGenerator = playerGenerator;
        }

        public async Task<PlayerDetailsDto> GetPlayerDetailsAsync(int id)
        {
            var player = await _context.Players
                .Include(p => p.Club)
                .Include(p => p.Agency)
                .Include(p => p.Attributes)
                .Include(p => p.Position)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (player == null) return null;

            return new PlayerDetailsDto
            {
                Id = player.Id,
                Name = player.Name,
                Age = player.Age,
                Nationality = player.Nationality,
                Position = player.Position?.Abbreviation ?? "UNK",
                OVR = player.CurrentAbility,
                POT = player.PotentialAbility,

                Pace = player.Attributes.Pace,
                Shooting = player.Attributes.Shooting,
                Passing = player.Attributes.Passing,
                Dribbling = player.Attributes.Dribbling,
                Defending = player.Attributes.Defending,
                Physical = player.Attributes.Physical,

                Ambition = player.Attributes.Ambition,
                Greed = player.Attributes.Greed,
                Loyalty = player.Attributes.Loyalty,

                MarketValue = player.MarketValue,
                Form = player.Form,
                ClubId = player.ClubId,
                ClubName = player.Club?.Name,
                AgencyName = player.Agency?.Name
            };
        }

        public async Task<(bool Success, string Message)> GenerateFreeAgentsAsync(int count)
        {
            var positions = await _context.Positions.ToListAsync();
            if (!positions.Any())
            {
                return (false, "Positions not imported. Please import positions first.");
            }

            var newPlayers = _playerGenerator.GenerateMultiplePlayers(count, "", null, positions);

            _context.Players.AddRange(newPlayers);
            await _context.SaveChangesAsync();

            return (true, $"Successfully generated {count} organic free agents into the world!");
        }

        public async Task<PaginatedResultDto<ScoutingPlayerDto>> GetScoutingPoolAsync(
            string? search, string? position, string? nationality,
            int? minAge, int? maxAge, decimal? maxValue,
            bool? hasAgency, string? sortBy, int page, int pageSize)
        {
            var query = _context.Players
                .Include(p => p.Club)
                .Include(p => p.Position)
                .AsQueryable();

            if (hasAgency.HasValue) query = hasAgency.Value ? query.Where(p => p.AgencyId != null) : query.Where(p => p.AgencyId == null);
            if (!string.IsNullOrEmpty(search)) query = query.Where(p => p.Name.Contains(search));
            if (!string.IsNullOrEmpty(position) && position != "All") query = query.Where(p => p.Position.Abbreviation == position);
            if (!string.IsNullOrEmpty(nationality)) query = query.Where(p => p.Nationality.Contains(nationality));
            if (minAge.HasValue) query = query.Where(p => p.Age >= minAge.Value);
            if (maxAge.HasValue) query = query.Where(p => p.Age <= maxAge.Value);
            if (maxValue.HasValue) query = query.Where(p => p.MarketValue <= maxValue.Value);

            query = sortBy switch
            {
                "Value" => query.OrderByDescending(p => p.MarketValue),
                "ValueAsc" => query.OrderBy(p => p.MarketValue),
                "Age" => query.OrderBy(p => p.Age),
                "AgeDesc" => query.OrderByDescending(p => p.Age),
                _ => query.OrderByDescending(p => p.MarketValue)
            };

            var totalCount = await query.CountAsync();

            var players = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ScoutingPlayerDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Position = p.Position.Abbreviation,
                    Age = p.Age,
                    Nationality = p.Nationality,
                    MarketValue = p.MarketValue,
                    ClubName = p.Club != null ? p.Club.Name : "Free Agent",
                    HasAgency = p.AgencyId != null
                })
                .ToListAsync();

            return new PaginatedResultDto<ScoutingPlayerDto>
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                Items = players
            };
        }
    }
}