namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Api.DTOs;
    using TenPercent.Application.DTOs;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.Enums;
    using TenPercent.Data.Models;

    public class AgencyService : IAgencyService
    {
        private readonly AppDbContext _context;
        private readonly IFinanceService _financeService;
        private readonly Random _rand = new Random();

        public AgencyService(AppDbContext context, IFinanceService financeService)
        {
            _context = context;
            _financeService = financeService;
        }

        public async Task<AgencyDto> GetMyAgencyAsync(int userId)
        {
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .ThenInclude(ag => ag.Players)
                .ThenInclude(p => p.ClubContracts.Where(cc => cc.IsActive))
                .Include(a => a.Agency)
                .ThenInclude(ag => ag.Players)
                .ThenInclude(p => p.RepresentationContracts.Where(rc => rc.IsActive))
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent?.Agency == null)
                return null;

            var activePlayers = agent.Agency.Players.Where(p => p.ClubContracts.Any() && p.RepresentationContracts.Any()).ToList();

            decimal totalProjectedIncome = 0;
            decimal totalContractsValue = 0;
            string topEarner = "N/A";
            decimal maxEarnerIncome = 0;

            foreach (var player in activePlayers)
            {
                var clubContract = player.ClubContracts.First();
                var repContract = player.RepresentationContracts.First();

                decimal weeklyCommission = clubContract.WeeklyWage * (repContract.IncomeCommissionPercentage / 100m);
                decimal projectedSeasonCommission = weeklyCommission * 40;
                totalProjectedIncome += projectedSeasonCommission;

                int remainingSeasons = clubContract.EndSeasonNumber - clubContract.StartSeasonNumber;
                decimal contractValue = (clubContract.WeeklyWage * 40 * (remainingSeasons > 0 ? remainingSeasons : 1)) + clubContract.ReleaseClause;
                totalContractsValue += contractValue;

                if (weeklyCommission > maxEarnerIncome)
                {
                    maxEarnerIncome = weeklyCommission;
                    topEarner = player.Name;
                }
            }

            return new AgencyDto
            {
                Id = agent.Agency.Id,
                Name = agent.Agency.Name,
                AgentName = agent.Name,
                LogoId = agent.Agency.LogoId,
                Budget = agent.Agency.Budget,
                Reputation = agent.Agency.Reputation,
                Level = agent.Agency.Level,
                EstablishedAt = agent.Agency.EstablishedAt,
                TotalPlayersCount = agent.Agency.Players.Count,
                ProjectedSeasonIncome = totalProjectedIncome,
                TotalContractsValue = totalContractsValue,
                TopEarnerName = topEarner
            };
        }

        public async Task<IEnumerable<AgencyPlayerDto>> GetAgencyPlayersAsync(int userId)
        {
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent?.Agency == null)
                return null;

            // Взимаме текущия сезон
            var worldState = await _context.WorldStates.FirstOrDefaultAsync();
            int currentSeasonNumber = 1;
            if (worldState != null && worldState.CurrentSeasonId.HasValue)
            {
                var activeSeason = await _context.Seasons.FindAsync(worldState.CurrentSeasonId.Value);
                if (activeSeason != null) currentSeasonNumber = activeSeason.SeasonNumber;
            }

            var players = await _context.Players
                .Include(p => p.Position)
                .Include(p => p.Club)
                .Include(p => p.Attributes)
                .Include(p => p.ClubContracts.Where(c => c.IsActive))
                .Include(p => p.RepresentationContracts.Where(c => c.IsActive))
                .Where(p => p.AgencyId == agent.Agency.Id)
                .Select(p => new AgencyPlayerDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Pos = p.Position.Abbreviation,
                    Age = p.Age,
                    Nationality = p.Nationality,
                    ClubName = p.Club != null ? p.Club.Name : "Free Agent",

                    Skill = p.CurrentAbility,
                    Value = p.MarketValue,
                    Wage = p.ClubContracts.Any() ? p.ClubContracts.First().WeeklyWage : 0,

                    // Изчисляване на оставащите сезони
                    ClubContractYearsLeft = p.ClubContracts.Any() ? Math.Max(0, p.ClubContracts.First().EndSeasonNumber - currentSeasonNumber) : 0,
                    AgencyContractYearsLeft = p.RepresentationContracts.Any() ? Math.Max(0, p.RepresentationContracts.First().EndSeasonNumber - currentSeasonNumber) : 0,

                    // Атрибути
                    Pace = p.Attributes.Pace,
                    Shooting = p.Attributes.Shooting,
                    Passing = p.Attributes.Passing,
                    Dribbling = p.Attributes.Dribbling,
                    Defending = p.Attributes.Defending,
                    Physical = p.Attributes.Physical,
                    Goalkeeping = p.Attributes.Goalkeeping,
                    Vision = p.Attributes.Vision,
                    Stamina = p.Attributes.Stamina
                })
                .ToListAsync();

            return players;
        }

        public async Task<(bool Success, string Message, bool Accepted)> OfferRepresentationAsync(int userId, OfferRepresentationDto dto)
        {
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent?.Agency == null)
                return (false, "Агенцията не е намерена.", false);

            var agency = agent.Agency;

            var player = await _context.Players
                .Include(p => p.Attributes)
                .Include(p => p.RepresentationContracts)
                .FirstOrDefaultAsync(p => p.Id == dto.PlayerId);

            if (player == null)
                return (false, "Играчът не съществува.", false);

            if (agency.Budget < dto.SigningBonusPaid)
                return (false, "Нямате достатъчно бюджет за този Signing Bonus.", false);

            if (player.AgencyId == agency.Id)
                return (false, "Този играч вече е ваш клиент. Използвайте опцията за подновяване на договор.", false);

            if (dto.WageCommissionPercentage > 15m || dto.TransferCommissionPercentage > 15m)
                return (false, "ФИФА не позволява комисионни над 15%.", false);

            double acceptanceChance = 50.0;
            double expectedReputation = player.CurrentAbility * 10;
            double reputationDiff = agency.Reputation - expectedReputation;
            acceptanceChance += (reputationDiff / 100.0) * 5.0;

            double greedFactor = player.Attributes.Greed / 100.0;
            double totalCommission = (double)(dto.WageCommissionPercentage + dto.TransferCommissionPercentage);
            acceptanceChance -= (totalCommission * 2.0) * (1.0 + greedFactor);

            double bonusToValueRatio = (double)(dto.SigningBonusPaid / (player.MarketValue + 1m));
            double bonusBoost = (bonusToValueRatio * 1000.0) * (1.0 + greedFactor);
            acceptanceChance += Math.Min(bonusBoost, 40.0);

            if (player.AgencyId.HasValue)
            {
                double loyaltyFactor = player.Attributes.Loyalty / 100.0;
                acceptanceChance -= (10.0 + (30.0 * loyaltyFactor));
            }

            int roll = _rand.Next(1, 101);
            acceptanceChance = Math.Clamp(acceptanceChance, 1.0, 99.0);
            bool isAccepted = roll <= acceptanceChance;

            if (!isAccepted)
            {
                return (true, $"{player.Name} отхвърли вашата оферта. (Шанс: {acceptanceChance:F1}%, Зар: {roll})", false);
            }

            var worldState = await _context.WorldStates.FirstOrDefaultAsync();
            int currentSeasonNumber = 1;
            if (worldState != null && worldState.CurrentSeasonId.HasValue)
            {
                var activeSeason = await _context.Seasons.FindAsync(worldState.CurrentSeasonId.Value);
                if (activeSeason != null) currentSeasonNumber = activeSeason.SeasonNumber;
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                agency.Budget -= dto.SigningBonusPaid;

                if (player.AgencyId.HasValue)
                {
                    var oldContract = player.RepresentationContracts.FirstOrDefault(c => c.IsActive && c.AgencyId == player.AgencyId);
                    if (oldContract != null) oldContract.IsActive = false;
                }

                var newContract = new RepresentationContract
                {
                    PlayerId = player.Id,
                    AgencyId = agency.Id,
                    StartSeasonNumber = currentSeasonNumber,
                    EndSeasonNumber = currentSeasonNumber + dto.DurationYears,
                    IncomeCommissionPercentage = dto.WageCommissionPercentage,
                    TransferCommissionPercentage = dto.TransferCommissionPercentage,
                    SigningBonusPaid = dto.SigningBonusPaid,
                    AgencyReleaseClause = dto.AgencyReleaseClause,
                    IsActive = true
                };

                _context.RepresentationContracts.Add(newContract);
                player.AgencyId = agency.Id;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Успех! {player.Name} подписа с вашата агенция!", true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Грешка при подписването на договора: " + ex.Message, false);
            }
        }

        public async Task<(bool Success, string Message)> CreateAgencyAsync(CreateAgencyDto dto)
        {
            var user = await _context.Users
         .Include(u => u.Agent)
         .FirstOrDefaultAsync(u => u.Id == dto.UserId);

            if (user == null) return (false, "User not found.");
            if (user.Agent != null) return (false, "This user already has an agency.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var agent = new Agent
                {
                    Name = dto.AgentName,
                    UserId = user.Id
                };

                var agency = new Agency
                {
                    Name = dto.AgencyName,
                    LogoId = dto.LogoId,
                    Agent = agent,
                    Budget = 0m
                };

                _context.Agents.Add(agent);
                _context.Agencies.Add(agency);

                await _context.SaveChangesAsync();

                var settings = await _context.EconomySettings.FirstOrDefaultAsync();
                decimal grantAmount = settings?.AgencyStartupGrant ?? 1_000_000m;

                var bank = await _context.Banks.FirstOrDefaultAsync();
                if (bank != null)
                {
                    var financeResult = await _financeService.ProcessTransactionAsync(
                        EntityType.Bank, bank.Id,
                        EntityType.Agency, agency.Id,
                        grantAmount,
                        TransactionCategory.StartupGrant,
                        "Welcome Solidarity Grant from World Central Bank"
                    );

                    if (!financeResult.Success) throw new Exception(financeResult.Message);
                }

                await transaction.CommitAsync();
                return (true, "Agency created successfully! Welcome to the game.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error creating agency: {ex.Message}");
            }
        }
        public async Task<AgencyFinanceDto?> GetAgencyFinanceAsync(int userId)
        {
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent?.Agency == null) return null;

            int agencyId = agent.Agency.Id;

            // Взимаме ВСИЧКИ транзакции за тази агенция, заедно със Сезона
            var allTransactions = await _context.Transactions
                .Include(t => t.Season)
                .Where(t => (t.SenderType == EntityType.Agency && t.SenderId == agencyId) ||
                            (t.ReceiverType == EntityType.Agency && t.ReceiverId == agencyId))
                .ToListAsync();

            // 1. Изолираме стартовия капитал (ПРОВЕРЯВАМЕ И ТИПА!)
            decimal startupCapital = allTransactions
                .Where(t => t.ReceiverType == EntityType.Agency && t.ReceiverId == agencyId && t.Category == TransactionCategory.StartupGrant)
                .Sum(t => t.Amount);

            // 2. Оперативни приходи (Всичко БЕЗ грантовете)
            decimal operatingIncome = allTransactions
                .Where(t => t.ReceiverType == EntityType.Agency && t.ReceiverId == agencyId && t.Category != TransactionCategory.StartupGrant)
                .Sum(t => t.Amount);

            // 3. Оперативни разходи (Внимаваме изпращачът да е АГЕНЦИЯ, а не Банката с ID 1)
            decimal operatingExpenses = allTransactions
                .Where(t => t.SenderType == EntityType.Agency && t.SenderId == agencyId)
                .Sum(t => t.Amount);

            // 4. ГРУПИРАНЕ ПО СЕЗОНИ (Само оперативни транзакции)
            var seasonalRecords = allTransactions
                .Where(t => t.SeasonId.HasValue && t.Category != TransactionCategory.StartupGrant)
                .GroupBy(t => new { t.SeasonId, t.Season!.SeasonNumber })
                .Select(g => new SeasonFinanceSummaryDto
                {
                    SeasonId = g.Key.SeasonId.Value,
                    SeasonNumber = g.Key.SeasonNumber,
                    // Отново задължително филтрираме по Тип и ID
                    Income = g.Where(t => t.ReceiverType == EntityType.Agency && t.ReceiverId == agencyId).Sum(t => t.Amount),
                    Expenses = g.Where(t => t.SenderType == EntityType.Agency && t.SenderId == agencyId).Sum(t => t.Amount)
                })
                .OrderByDescending(s => s.SeasonNumber) // Най-новите сезони първи
                .ToList();

            // 5. Последните 30 транзакции за лога
            var recentTransactions = allTransactions
                .OrderByDescending(t => t.Date)
                .Take(30)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Type = (t.ReceiverType == EntityType.Agency && t.ReceiverId == agencyId) ? "income" : "expense",
                    Description = t.Description,
                    Amount = t.Amount,
                    Date = t.Date
                }).ToList();

            return new AgencyFinanceDto
            {
                Balance = agent.Agency.Budget,
                StartupCapital = startupCapital,
                OperatingIncome = operatingIncome,
                OperatingExpenses = operatingExpenses,
                SeasonalRecords = seasonalRecords,
                RecentTransactions = recentTransactions
            };
        }
    }
}