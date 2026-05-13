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
            var worldState = await _context.WorldStates.FirstOrDefaultAsync();
            var activeSeasonId = worldState?.CurrentSeasonId;

            var player = await _context.Players
                .Include(p => p.Club)
                .Include(p => p.Agency)
                .Include(p => p.Attributes)
                .Include(p => p.Position)
                .Include(p => p.ClubContracts.Where(c => c.IsActive))
                .Include(p => p.SeasonPerformances.Where(sp => sp.SeasonId == activeSeasonId))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (player == null) return null;

            var recentMatches = await _context.PlayerMatchPerformances
                .Include(m => m.Fixture)
                    .ThenInclude(f => f.HomeClub)
                .Include(m => m.Fixture)
                    .ThenInclude(f => f.AwayClub)
                .Where(m => m.PlayerId == id && m.Fixture.IsPlayed)
                .OrderByDescending(m => m.Fixture.ScheduledDate)
                .Take(5)
                .ToListAsync();

            var activeContract = player.ClubContracts.FirstOrDefault();
            var currentSeasonStats = player.SeasonPerformances.FirstOrDefault();

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
                AgencyId = player.AgencyId,
                AgencyName = player.Agency?.Name,
                HasAgent = player.AgencyId != null,
                WeeklyWage = activeContract?.WeeklyWage ?? 0,
                ContractYearsLeft = activeContract != null ? (activeContract.EndSeasonNumber - activeContract.StartSeasonNumber) : 0,
                SeasonAppearances = currentSeasonStats?.Appearances ?? 0,
                SeasonGoals = currentSeasonStats?.Goals ?? 0,
                SeasonAssists = currentSeasonStats?.Assists ?? 0,
                SeasonAverageRating = currentSeasonStats?.AverageRating ?? 0,
                SeasonYellowCards = currentSeasonStats?.YellowCards ?? 0,
                SeasonRedCards = currentSeasonStats?.RedCards ?? 0,
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
        public async Task ProcessYearlyProgressionAsync()
        {
            // Взимаме всички играчи с техните атрибути и позиции (за да можем да сметнем OVR накрая)
            var players = await _context.Players
                .Include(p => p.Attributes)
                .Include(p => p.Position)
                .ToListAsync();

            var rand = new Random();

            foreach (var player in players)
            {
                player.Age += 1;
                var attr = player.Attributes;

                // ----------------------------------------------------
                // 1. ПРОГРЕСИЯ НА МЛАДИ ИГРАЧИ (Под 26 години)
                // ----------------------------------------------------
                if (player.Age <= 26)
                {
                    // Помощна функция: ВРЪЩА новата стойност
                    int GrowAttribute(int current, int potential, int maxGrowth)
                    {
                        if (current < potential)
                        {
                            // Шанс да се вдигне с от 1 до maxGrowth точки (ако не надминава потенциала)
                            int growth = rand.Next(1, maxGrowth + 1);
                            return Math.Min(potential, current + growth);
                        }
                        return current;
                    }

                    // Колкото по-млад е, толкова по-бързо се развива
                    int growthSpeed = player.Age < 21 ? 3 : (player.Age < 24 ? 2 : 1);

                    // Пренасочваме резултата обратно към property-то
                    attr.Pace = GrowAttribute(attr.Pace, attr.PotentialPace, growthSpeed);
                    attr.Shooting = GrowAttribute(attr.Shooting, attr.PotentialShooting, growthSpeed);
                    attr.Passing = GrowAttribute(attr.Passing, attr.PotentialPassing, growthSpeed);
                    attr.Dribbling = GrowAttribute(attr.Dribbling, attr.PotentialDribbling, growthSpeed);
                    attr.Defending = GrowAttribute(attr.Defending, attr.PotentialDefending, growthSpeed);
                    attr.Physical = GrowAttribute(attr.Physical, attr.PotentialPhysical, growthSpeed);
                    attr.Goalkeeping = GrowAttribute(attr.Goalkeeping, attr.PotentialGoalkeeping, growthSpeed);
                    attr.Vision = GrowAttribute(attr.Vision, attr.PotentialVision, growthSpeed);
                    attr.Stamina = GrowAttribute(attr.Stamina, attr.PotentialStamina, growthSpeed);
                }

                // ----------------------------------------------------
                // 2. ДЕГРАДАЦИЯ (РЕГРЕСИЯ) НА ВЕТЕРАНИ (Над 30 години)
                // ----------------------------------------------------
                else if (player.Age >= 31)
                {
                    // Помощна функция: ВРЪЩА новата по-ниска стойност
                    int DeclineAttribute(int current, int maxDecline)
                    {
                        // Шанс да се свали с от 0 до maxDecline точки
                        int decline = rand.Next(0, maxDecline + 1);
                        return Math.Max(1, current - decline); // Не пада под 1
                    }

                    // След 30 г. деградацията е бавна, след 33 става по-агресивна
                    int declineSpeed = player.Age > 33 ? 3 : 1;

                    // Физическите атрибути падат най-бързо!
                    attr.Pace = DeclineAttribute(attr.Pace, declineSpeed + 1);
                    attr.Stamina = DeclineAttribute(attr.Stamina, declineSpeed + 1);
                    attr.Physical = DeclineAttribute(attr.Physical, declineSpeed);

                    // Техническите и менталните падат много по-бавно (или изобщо не)
                    if (player.Age > 34)
                    {
                        attr.Dribbling = DeclineAttribute(attr.Dribbling, 1);
                        attr.Shooting = DeclineAttribute(attr.Shooting, 1);
                        attr.Defending = DeclineAttribute(attr.Defending, 1);
                    }
                }

                // ----------------------------------------------------
                // 3. ПРЕИЗЧИСЛЯВАНЕ НА OVR И ПАЗАРНА ЦЕНА
                // ----------------------------------------------------
                player.RecalculateCurrentAbility();

                // Формула за цена
                decimal valueBase = player.CurrentAbility * 100000m;
                decimal potBonus = Math.Max(0, player.PotentialAbility - player.CurrentAbility) * 50000m;

                // Възрастова премия: Младите са скъпи, старите са евтини
                decimal youthPremium = 1.0m;
                if (player.Age < 21) youthPremium = 1.5m;
                else if (player.Age < 24) youthPremium = 1.2m;
                else if (player.Age > 33) youthPremium = 0.4m;
                else if (player.Age > 30) youthPremium = 0.7m;

                player.MarketValue = (valueBase + potBonus) * youthPremium;
            }

            // Записваме всички промени в базата
            _context.Players.UpdateRange(players);
            await _context.SaveChangesAsync();
        }
    }
}
