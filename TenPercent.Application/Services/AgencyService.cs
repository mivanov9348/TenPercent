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
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent?.Agency == null)
                return null;

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
                TotalPlayersCount = agent.Agency.Players.Count
            };
        }

        public async Task<IEnumerable<AgencyPlayerDto>> GetAgencyPlayersAsync(int userId)
        {
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent?.Agency == null)
                return null;

            var players = await _context.Players
                .Include(p => p.Position)
                .Where(p => p.AgencyId == agent.Agency.Id)
                .Select(p => new AgencyPlayerDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Pos = p.Position.Abbreviation,
                    Age = p.Age,
                    Skill = p.CurrentAbility,
                    Potential = p.PotentialAbility,
                    Value = p.MarketValue,
                    Form = p.Form ?? "Average"
                })
                .ToListAsync();

            return players;
        }

        public async Task<(bool Success, string Message, bool Accepted)> OfferRepresentationAsync(int userId, OfferRepresentationDto dto)
        {
            // 1. Намираме Агенцията
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent?.Agency == null)
                return (false, "Агенцията не е намерена.", false);

            var agency = agent.Agency;

            // 2. Намираме Играча (С неговите атрибути)
            var player = await _context.Players
                .Include(p => p.Attributes)
                .Include(p => p.RepresentationContracts)
                .FirstOrDefaultAsync(p => p.Id == dto.PlayerId);

            if (player == null)
                return (false, "Играчът не съществува.", false);

            // 3. Базови Валидации
            if (agency.Budget < dto.SigningBonusPaid)
                return (false, "Нямате достатъчно бюджет за този Signing Bonus.", false);

            if (player.AgencyId == agency.Id)
                return (false, "Този играч вече е ваш клиент. Използвайте опцията за подновяване на договор.", false);

            if (dto.WageCommissionPercentage > 15m || dto.TransferCommissionPercentage > 15m)
                return (false, "ФИФА не позволява комисионни над 15%.", false);

            // ==========================================
            // 4. МАТЕМАТИКАТА ЗА ПРЕГОВОРИТЕ (AI LOGIC)
            // ==========================================

            // Базов шанс за подписване (започва от 50%)
            double acceptanceChance = 50.0;

            // А. Влияние на Репутацията срещу OVR на играча
            // Ако си Ниво 1 (Rep 0), а той е 90 OVR, разликата е огромна.
            // Приемаме, че 1 OVR = 10 Репутация за баланс.
            double expectedReputation = player.CurrentAbility * 10;
            double reputationDiff = agency.Reputation - expectedReputation;

            // За всеки 100 точки разлика шансът се променя с 5%
            acceptanceChance += (reputationDiff / 100.0) * 5.0;

            // Б. Влияние на Алчността (Greed) и Комисионните
            // Колкото е по-алчен, толкова повече мрази ти да взимаш висок процент
            double greedFactor = player.Attributes.Greed / 100.0; // от 0.01 до 1.00
            double totalCommission = (double)(dto.WageCommissionPercentage + dto.TransferCommissionPercentage);

            // Наказание за високи комисионни, умножено по алчността
            acceptanceChance -= (totalCommission * 2.0) * (1.0 + greedFactor);

            // В. Влияние на Signing Bonus
            // Сравняваме бонуса с пазарната му цена. Алчните играчи искат по-голям бонус!
            double bonusToValueRatio = (double)(dto.SigningBonusPaid / (player.MarketValue + 1m)); // +1 против делене на 0

            // Ако му дадеш 10% от пазарната му цена като бонус, това е огромен плюс
            double bonusBoost = (bonusToValueRatio * 1000.0) * (1.0 + greedFactor);
            acceptanceChance += Math.Min(bonusBoost, 40.0); // Ограничаваме бууста до макс 40%

            // Г. Опит за "Кражба" на играч (Ако има друга агенция)
            if (player.AgencyId.HasValue)
            {
                double loyaltyFactor = player.Attributes.Loyalty / 100.0;
                // Наказание между 10% и 40% в зависимост от лоялността му към сегашния агент
                acceptanceChance -= (10.0 + (30.0 * loyaltyFactor));
            }

            // ==========================================
            // 5. РЕШЕНИЕТО НА ИГРАЧА
            // ==========================================

            // Хвърляме зар от 1 до 100
            int roll = _rand.Next(1, 101);

            // Ограничаваме шанса между 1% и 99% (винаги има шанс за изненада)
            acceptanceChance = Math.Clamp(acceptanceChance, 1.0, 99.0);

            bool isAccepted = roll <= acceptanceChance;

            if (!isAccepted)
            {
                // Играчът отказва. 
                return (true, $"{player.Name} отхвърли вашата оферта. (Шанс: {acceptanceChance:F1}%, Зар: {roll})", false);
            }

            // ==========================================
            // 6. ИГРАЧЪТ ПРИЕМА - ЗАПИСВАМЕ В БАЗАТА
            // ==========================================

            // Създаваме транзакция, защото пипаме пари и договори едновременно
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Взимаме му парите от бюджета
                agency.Budget -= dto.SigningBonusPaid;

                // 2. Прекратяваме стария му договор с друга агенция (ако има такъв)
                if (player.AgencyId.HasValue)
                {
                    var oldContract = player.RepresentationContracts.FirstOrDefault(c => c.IsActive && c.AgencyId == player.AgencyId);
                    if (oldContract != null) oldContract.IsActive = false;
                }

                // 3. Създаваме новия договор
                var newContract = new RepresentationContract
                {
                    PlayerId = player.Id,
                    AgencyId = agency.Id,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(dto.DurationYears),
                    WageCommissionPercentage = dto.WageCommissionPercentage,
                    TransferCommissionPercentage = dto.TransferCommissionPercentage,
                    SigningBonusPaid = dto.SigningBonusPaid,
                    AgencyReleaseClause = dto.AgencyReleaseClause,
                    IsActive = true
                };

                _context.RepresentationContracts.Add(newContract);

                // 4. Закачаме го официално към твоята агенция
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
            var user = await _context.GameUsers
         .Include(u => u.Agent)
         .FirstOrDefaultAsync(u => u.Id == dto.UserId);

            if (user == null) return (false, "User not found.");

            if (user.Agent != null) return (false, "This user already has an agency.");

            // Използваме транзакция, за да сме сигурни, че ако Банката няма пари (или гръмне), агенцията няма да се създаде
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
                    Budget = 0m // Стартира на 0
                };

                _context.Agents.Add(agent);
                _context.Agencies.Add(agency);

                // Запазваме, за да генерираме ID на агенцията, което ни трябва за транзакцията
                await _context.SaveChangesAsync();

                // Взимаме настройките за гранта
                var settings = await _context.EconomySettings.FirstOrDefaultAsync();
                decimal grantAmount = settings?.AgencyStartupGrant ?? 1_000_000m;

                var bank = await _context.Banks.FirstOrDefaultAsync();
                if (bank != null)
                {
                    // Банката превежда парите
                    var financeResult = await _financeService.ProcessTransactionAsync(
                        EntityType.Bank, bank.Id,
                        EntityType.Agency, agency.Id,
                        grantAmount,
                        TransactionCategory.StartupGrant, // Използвай новия Enum
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

            // Взимаме последните 20 транзакции, в които Агенцията е участвала
            var recentTransactions = await _context.Transactions
                .Where(t => (t.SenderType == EntityType.Agency && t.SenderId == agencyId) ||
                            (t.ReceiverType == EntityType.Agency && t.ReceiverId == agencyId))
                .OrderByDescending(t => t.Date)
                .Take(20)
                .ToListAsync();

            // Смятаме общите приходи и разходи за статистиката
            decimal totalIncome = await _context.Transactions
                .Where(t => t.ReceiverType == EntityType.Agency && t.ReceiverId == agencyId)
                .SumAsync(t => t.Amount);

            decimal totalExpenses = await _context.Transactions
                .Where(t => t.SenderType == EntityType.Agency && t.SenderId == agencyId)
                .SumAsync(t => t.Amount);

            return new AgencyFinanceDto
            {
                Balance = agent.Agency.Budget,
                TotalIncome = totalIncome,
                TotalExpenses = totalExpenses,
                RecentTransactions = recentTransactions.Select(t => new TransactionDto
                {
                    Id = t.Id,
                    // Ако Агенцията е получател, значи е приход. Иначе е разход.
                    Type = (t.ReceiverType == EntityType.Agency && t.ReceiverId == agencyId) ? "income" : "expense",
                    Description = t.Description,
                    Amount = t.Amount,
                    Date = t.Date
                }).ToList()
            };
        }
    }
}