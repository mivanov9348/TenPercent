namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.Enums;
    using TenPercent.Data.Models;
    using TenPercent.Data.Models.Finance;

    public class FinanceService : IFinanceService
    {
        private readonly AppDbContext _context;
        private readonly IMessageService _messageService; // НОВО
        private readonly Random _rand = new Random();

        // НОВО: Инжектираме IMessageService
        public FinanceService(AppDbContext context, IMessageService messageService)
        {
            _context = context;
            _messageService = messageService;
        }

        public async Task<(bool Success, string Message)> ProcessTransactionAsync(
            EntityType senderType, int? senderId,
            EntityType receiverType, int? receiverId,
            decimal amount, TransactionCategory category, string description)
        {
            if (amount <= 0) return (false, "Сумата трябва да е по-голяма от 0.");

            bool isNestedTransaction = _context.Database.CurrentTransaction != null;
            var dbTransaction = isNestedTransaction ? null : await _context.Database.BeginTransactionAsync();

            try
            {
                var worldState = await _context.WorldStates.FirstOrDefaultAsync();
                int? currentSeasonId = worldState?.CurrentSeasonId;

                var senderResult = await UpdateEntityBalance(senderType, senderId, -amount, category);
                if (!senderResult.Success) return senderResult;

                var receiverResult = await UpdateEntityBalance(receiverType, receiverId, amount, category);
                if (!receiverResult.Success) return receiverResult;

                var transactionLog = new Transaction
                {
                    Date = DateTime.UtcNow,
                    SeasonId = currentSeasonId,
                    Amount = amount,
                    SenderType = senderType,
                    SenderId = senderId,
                    ReceiverType = receiverType,
                    ReceiverId = receiverId,
                    Category = category,
                    Description = description,
                };

                _context.Transactions.Add(transactionLog);
                await _context.SaveChangesAsync();

                if (senderType != EntityType.Bank && receiverType != EntityType.Bank && category != TransactionCategory.Tax)
                {
                    await ApplyGlobalIncomeTaxAsync(receiverType, receiverId.Value, amount, transactionLog.Id, currentSeasonId);
                }

                if (dbTransaction != null) await dbTransaction.CommitAsync();

                return (true, "Транзакцията е успешна.");
            }
            catch (Exception ex)
            {
                if (dbTransaction != null) await dbTransaction.RollbackAsync();
                return (false, "Грешка при счетоводната транзакция: " + ex.Message);
            }
            finally
            {
                if (dbTransaction != null) await dbTransaction.DisposeAsync();
            }
        }

        private async Task ApplyGlobalIncomeTaxAsync(EntityType taxpayerType, int taxpayerId, decimal incomeAmount, int originalTxId, int? seasonId)
        {
            var settings = await _context.EconomySettings.FirstOrDefaultAsync();
            decimal taxRate = settings?.GlobalIncomeTax ?? 0.10m;

            if (taxRate <= 0) return;

            decimal taxAmount = Math.Round(incomeAmount * taxRate, 2);

            var bank = await _context.Banks.FirstOrDefaultAsync();
            if (bank == null || taxAmount <= 0) return;

            await UpdateEntityBalance(taxpayerType, taxpayerId, -taxAmount, TransactionCategory.Tax);
            await UpdateEntityBalance(EntityType.Bank, bank.Id, taxAmount, TransactionCategory.Tax);

            var taxLog = new Transaction
            {
                Date = DateTime.UtcNow,
                SeasonId = seasonId,
                Amount = taxAmount,
                SenderType = taxpayerType,
                SenderId = taxpayerId,
                ReceiverType = EntityType.Bank,
                ReceiverId = bank.Id,
                Category = TransactionCategory.Tax,
                Description = $"Global Income Tax ({taxRate * 100}%) on revenue from Tx #{originalTxId}"
            };

            _context.Transactions.Add(taxLog);
            await _context.SaveChangesAsync();
        }

        private async Task<(bool Success, string Message)> UpdateEntityBalance(EntityType type, int? id, decimal amount, TransactionCategory category)
        {
            if (type == EntityType.Bank)
            {
                var bank = await _context.Banks.FirstOrDefaultAsync();
                if (bank == null) return (false, "Банката не съществува.");
                bank.ReserveBalance += amount;
            }
            else if (type == EntityType.Club && id.HasValue)
            {
                var club = await _context.Clubs.FindAsync(id.Value);
                if (club == null) return (false, "Клубът не е намерен.");

                if (amount < 0)
                {
                    decimal absAmount = Math.Abs(amount);
                    if (category == TransactionCategory.Wage)
                    {
                        if (club.WageBudget >= absAmount)
                        {
                            club.WageBudget -= absAmount;
                        }
                        else
                        {
                            decimal remainder = absAmount - club.WageBudget;
                            if (club.TransferBudget >= remainder)
                            {
                                club.WageBudget = 0;
                                club.TransferBudget -= remainder;
                            }
                            else return (false, $"Недостатъчни средства в клуб {club.Name} (дори и с трансферния бюджет).");
                        }
                    }
                    else
                    {
                        if (club.TransferBudget < absAmount) return (false, $"Недостатъчни трансферни средства в {club.Name}.");
                        club.TransferBudget -= absAmount;
                    }
                }
                else
                {
                    club.TransferBudget += amount;
                }
            }
            else if (type == EntityType.Agency && id.HasValue)
            {
                var agency = await _context.Agencies.FindAsync(id.Value);
                if (agency == null) return (false, "Агенцията не е намерена.");

                agency.Budget += amount;
            }
            else if (type == EntityType.Player && id.HasValue)
            {
                var player = await _context.Players.FindAsync(id.Value);
                if (player == null) return (false, "Играчът не е намерен.");
                if (amount < 0 && player.Balance < Math.Abs(amount)) return (false, "Недостатъчни средства на играча.");
                player.Balance += amount;
            }

            return (true, string.Empty);
        }

        public async Task<(bool Success, string Message)> InitializeWorldEconomyAsync(decimal initialBankBudget)
        {
            var settings = await _context.EconomySettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new EconomySettings { InitialBankReserve = initialBankBudget };
                _context.EconomySettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            var bank = await _context.Banks.FirstOrDefaultAsync();
            if (bank == null)
            {
                bank = new Bank { Name = "World Central Bank", ReserveBalance = settings.InitialBankReserve };
                _context.Banks.Add(bank);
                await _context.SaveChangesAsync();
            }

            var unfundedClubs = await _context.Clubs
                .Where(c => c.TransferBudget == 0 && c.WageBudget == 0)
                .ToListAsync();

            if (!unfundedClubs.Any()) return (true, "Няма нови клубове за финансиране.");

            int fundedCount = 0;
            decimal totalDistributed = 0;

            foreach (var club in unfundedClubs)
            {
                decimal startCash = (club.Reputation * settings.ClubReputationMultiplier) + settings.ClubBaseGrant;

                var txResult = await ProcessTransactionAsync(
                                        EntityType.Bank, bank.Id,
                                        EntityType.Club, club.Id,
                                        startCash,
                                        TransactionCategory.InitialAllocation,
                                        $"Initial TV & Sponsor Rights for {club.Name}"
                );

                if (txResult.Success)
                {
                    club.WageBudget = startCash * settings.ClubWageBudgetPercentage;
                    _context.Clubs.Update(club);
                    fundedCount++;
                    totalDistributed += startCash;
                }
            }

            await _context.SaveChangesAsync();
            return (true, $"Успех! Раздадени {totalDistributed:N0} на {fundedCount} клуба.");
        }

        public async Task<(bool Success, string Message)> ProcessWeeklyWagesAsync()
        {
            var settings = await _context.EconomySettings.FirstOrDefaultAsync() ?? new EconomySettings();
            var bank = await _context.Banks.FirstOrDefaultAsync();
            var worldState = await _context.WorldStates.FirstOrDefaultAsync();

            if (bank == null) return (false, "Банката не съществува.");
            decimal taxRate = settings.GlobalIncomeTax > 0 ? settings.GlobalIncomeTax : 0.10m;

            int currentSeasonId = worldState?.CurrentSeasonId ?? 0;
            var activeSeason = currentSeasonId > 0
                ? await _context.Seasons.FindAsync(currentSeasonId)
                : null;

            int lastGameweek = (activeSeason?.CurrentGameweek ?? 1) - 1;

            var activeClubContracts = await _context.ClubContracts
                .Include(cc => cc.Club)
                .Include(cc => cc.Player).ThenInclude(p => p.Position)
                .Include(cc => cc.Player).ThenInclude(p => p.Attributes)
                .Include(cc => cc.Player).ThenInclude(p => p.RepresentationContracts.Where(rc => rc.IsActive))
                .Where(cc => cc.IsActive && cc.WeeklyWage > 0)
                .ToListAsync();

            var weeklyPerformances = await _context.PlayerMatchPerformances
                .Include(p => p.Fixture)
                .Where(p => p.Fixture.SeasonId == currentSeasonId && p.Fixture.Gameweek == lastGameweek)
                .ToDictionaryAsync(p => p.PlayerId);

            var activeAgencyIds = activeClubContracts
                .SelectMany(c => c.Player.RepresentationContracts.Select(rc => rc.AgencyId))
                .Distinct().ToList();

            var agencies = await _context.Agencies
                .Where(a => activeAgencyIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id);

            int processedWages = 0;
            int processedCommissions = 0;
            int processedTaxes = 0;
            int incidentCount = 0;
            decimal totalTaxCollected = 0m;
            decimal totalBonusesPaid = 0m;
            decimal totalFinesPaid = 0m;

            var transactionLogs = new List<Transaction>();
            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var contract in activeClubContracts)
                {
                    var club = contract.Club;
                    var player = contract.Player;

                    decimal totalPlayerIncome = contract.WeeklyWage;
                    string incomeDescription = $"Weekly wage for {player.Name}";

                    // --- МАЧОВИ БОНУСИ ---
                    if (weeklyPerformances.TryGetValue(player.Id, out var perf))
                    {
                        decimal matchBonus = 0m;
                        if (perf.MinutesPlayed > 0) matchBonus += contract.AppearanceBonus;
                        matchBonus += (perf.Goals * contract.GoalBonus);
                        matchBonus += (perf.Assists * contract.AssistBonus);

                        bool isDefenderOrGk = player.Position.Abbreviation == "GK" || player.Position.Abbreviation == "DEF";
                        if (isDefenderOrGk && perf.MinutesPlayed >= 60)
                        {
                            bool isCleanSheet = (perf.Fixture.HomeClubId == club.Id && perf.Fixture.AwayGoals == 0) ||
                                                (perf.Fixture.AwayClubId == club.Id && perf.Fixture.HomeGoals == 0);

                            if (isCleanSheet) matchBonus += contract.CleanSheetBonus;
                        }

                        if (matchBonus > 0)
                        {
                            totalPlayerIncome += matchBonus;
                            totalBonusesPaid += matchBonus;
                            incomeDescription = $"Wage + Match Bonuses for {player.Name}";
                        }
                    }

                    // --- ПЛАЩАНЕ ОТ КЛУБА ---
                    if (club.WageBudget >= totalPlayerIncome)
                    {
                        club.WageBudget -= totalPlayerIncome;
                    }
                    else
                    {
                        decimal remainder = totalPlayerIncome - club.WageBudget;
                        if (club.TransferBudget >= remainder)
                        {
                            club.WageBudget = 0;
                            club.TransferBudget -= remainder;
                        }
                        else continue;
                    }

                    player.Balance += totalPlayerIncome;
                    processedWages++;
                    transactionLogs.Add(CreateLog(currentSeasonId, EntityType.Club, club.Id, EntityType.Player, player.Id, totalPlayerIncome, TransactionCategory.Wage, incomeDescription));

                    // --- ДАНЪЦИ ИГРАЧ (ЗАКРИТИ ЗА QA) ---

                    // --- АГЕНТСКА КОМИСИОННА ---
                    var agentContract = player.RepresentationContracts.FirstOrDefault();
                    if (agentContract != null && agentContract.IncomeCommissionPercentage > 0)
                    {
                        var agency = agencies[agentContract.AgencyId];

                        decimal commissionAmount = Math.Round(totalPlayerIncome * (agentContract.IncomeCommissionPercentage / 100m), 2);

                        if (player.Balance >= commissionAmount)
                        {
                            player.Balance -= commissionAmount;
                            agency.Budget += commissionAmount;
                            processedCommissions++;
                            transactionLogs.Add(CreateLog(currentSeasonId, EntityType.Player, player.Id, EntityType.Agency, agency.Id, commissionAmount, TransactionCategory.Commission, $"Commission ({agentContract.IncomeCommissionPercentage}%) from {player.Name}"));

                            // НОВО: ПРАЩАМЕ СЪОБЩЕНИЕ ЗА ПОЛУЧЕНАТА КОМИСИОННА
                            var placeholders = new Dictionary<string, string>
                            {
                                { "PlayerName", player.Name },
                                { "Amount", commissionAmount.ToString("N0") }
                            };

                            await _messageService.SendTemplatedMessageAsync(
                                receiverAgencyId: agency.Id,
                                senderType: EntityType.System,
                                senderId: 0,
                                senderName: "Finance Dept.",
                                type: MessageType.Finance,
                                placeholders: placeholders,
                                relatedEntityId: player.Id,
                                templateCode: "COMMISSION" // Използваме кода от Excel таблицата
                            );

                            // --- ДАНЪК КОМИСИОННА (ЗАКРИТ ЗА QA) ---
                        }

                        // --- СИСТЕМА ЗА АГЕНТСКИ РИСК (ИНЦИДЕНТИ) - ЗАКРИТА ЗА QA ---
                    }
                }

                await _context.Transactions.AddRangeAsync(transactionLogs);
                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return (true, $"Платени {processedWages} заплати и {processedCommissions} комисионни. Събрани {totalTaxCollected:N0} данъци. Възникнаха {incidentCount} инцидента на стойност {totalFinesPaid:N0}.");
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return (false, $"Грешка при изплащането: {ex.Message}");
            }
        }

        private Transaction CreateLog(int? seasonId, EntityType sender, int senderId, EntityType receiver, int receiverId, decimal amount, TransactionCategory cat, string desc)
        {
            return new Transaction
            {
                Date = DateTime.UtcNow,
                SeasonId = seasonId,
                Amount = amount,
                SenderType = sender,
                SenderId = senderId,
                ReceiverType = receiver,
                ReceiverId = receiverId,
                Category = cat,
                Description = desc
            };
        }
    }
}