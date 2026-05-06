namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
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

        public FinanceService(AppDbContext context)
        {
            _context = context;
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
                // 1. ВАДИМ ПАРИ ОТ ИЗПРАЩАЧА
                var senderResult = await UpdateEntityBalance(senderType, senderId, -amount, category);
                if (!senderResult.Success) return senderResult;

                // 2. ДАВАМЕ ПАРИ НА ПОЛУЧАТЕЛЯ
                var receiverResult = await UpdateEntityBalance(receiverType, receiverId, amount, category);
                if (!receiverResult.Success) return receiverResult;

                // 3. ЗАПИСВАМЕ В ЖУРНАЛА
                var transactionLog = new Transaction
                {
                    Date = DateTime.UtcNow,
                    Amount = amount,
                    SenderType = senderType,
                    SenderId = senderId,
                    ReceiverType = receiverType,
                    ReceiverId = receiverId,
                    Category = category,
                    Description = description
                };
                _context.Transactions.Add(transactionLog);
                await _context.SaveChangesAsync();

                // ==========================================
                // 4. НОВО: УНИВЕРСАЛЕН ГЛОБАЛЕН ДАНЪК
                // ==========================================
                // Правила: Изпращачът не е Банката, Получателят не е Банката и не плащаме самия Данък в момента
                if (senderType != EntityType.Bank && receiverType != EntityType.Bank && category != TransactionCategory.Tax)
                {
                    await ApplyGlobalIncomeTaxAsync(receiverType, receiverId.Value, amount, transactionLog.Id);
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

        // ==========================================
        // НОВ УНИВЕРСАЛЕН МЕТОД ЗА ДАНЪЦИ
        // ==========================================
        private async Task ApplyGlobalIncomeTaxAsync(EntityType taxpayerType, int taxpayerId, decimal incomeAmount, int originalTxId)
        {
            var settings = await _context.EconomySettings.FirstOrDefaultAsync();
            decimal taxRate = settings?.GlobalIncomeTax ?? 0.10m; // Ползваме новото ти поле

            if (taxRate <= 0) return;

            decimal taxAmount = Math.Round(incomeAmount * taxRate, 2);

            var bank = await _context.Banks.FirstOrDefaultAsync();
            if (bank == null || taxAmount <= 0) return;

            // 1. Вадим данъка от този, който току-що е получил парите
            await UpdateEntityBalance(taxpayerType, taxpayerId, -taxAmount, TransactionCategory.Tax);

            // 2. Даваме ги на банката
            await UpdateEntityBalance(EntityType.Bank, bank.Id, taxAmount, TransactionCategory.Tax);

            // 3. Логваме данъчната транзакция
            var taxLog = new Transaction
            {
                Date = DateTime.UtcNow,
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
                if (amount < 0 && agency.Budget < Math.Abs(amount)) return (false, "Недостатъчен бюджет на агенцията.");
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

        // --- ИНИЦИАЛИЗАЦИЯ НА ИКОНОМИКАТА ---

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
                // Използваме настройките за формулата
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

        // --- ОБНОВЕН МЕТОД ЗА СЕДМИЧНИ ЗАПЛАТИ (МНОГО ПО-ЧИСТ) ---
        public async Task<(bool Success, string Message)> ProcessWeeklyWagesAsync()
        {
            // 1. Дърпаме глобалните настройки и Банката веднъж!
            var settings = await _context.EconomySettings.FirstOrDefaultAsync() ?? new EconomySettings();
            var bank = await _context.Banks.FirstOrDefaultAsync();
            if (bank == null) return (false, "Банката не съществува.");

            decimal taxRate = settings.GlobalIncomeTax;

            // 2. Дърпаме всички активни договори (Както досега)
            var activeClubContracts = await _context.ClubContracts
                .Include(cc => cc.Club)
                .Include(cc => cc.Player)
                    .ThenInclude(p => p.RepresentationContracts.Where(rc => rc.IsActive))
                .Where(cc => cc.IsActive && cc.WeeklyWage > 0)
                .ToListAsync();

            // 3. НОВО: Дърпаме Агенциите в паметта предварително, за да не ги търсим една по една
            var activeAgencyIds = activeClubContracts
                .SelectMany(c => c.Player.RepresentationContracts.Select(rc => rc.AgencyId))
                .Distinct()
                .ToList();
            var agencies = await _context.Agencies
                .Where(a => activeAgencyIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id);

            int processedWages = 0;
            int processedCommissions = 0;

            // 4. НОВО: Създаваме списък, в който ще трупаме лог-записите в паметта
            var transactionLogs = new List<Transaction>();

            // Започваме една обща DB транзакция за сигурност
            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var contract in activeClubContracts)
                {
                    var club = contract.Club;
                    var player = contract.Player;
                    decimal wage = contract.WeeklyWage;

                    // --- СТЪПКА 1: ПЛАЩАНЕ НА ЗАПЛАТАТА (Клуб -> Играч) ---
                    if (club.WageBudget >= wage)
                    {
                        club.WageBudget -= wage;
                    }
                    else
                    {
                        decimal remainder = wage - club.WageBudget;
                        if (club.TransferBudget >= remainder)
                        {
                            club.WageBudget = 0;
                            club.TransferBudget -= remainder;
                        }
                        else continue; // Клубът няма никакви пари - прескачаме плащането
                    }

                    player.Balance += wage;
                    processedWages++;
                    transactionLogs.Add(CreateLog(EntityType.Club, club.Id, EntityType.Player, player.Id, wage, TransactionCategory.Wage, $"Weekly wage for {player.Name}"));

                    // --- СТЪПКА 2: ДАНЪК ВЪРХУ ЗАПЛАТАТА (Играч -> Банка) ---
                    decimal wageTax = Math.Round(wage * taxRate, 2);
                    if (wageTax > 0)
                    {
                        player.Balance -= wageTax;
                        bank.ReserveBalance += wageTax;
                        transactionLogs.Add(CreateLog(EntityType.Player, player.Id, EntityType.Bank, bank.Id, wageTax, TransactionCategory.Tax, $"Income Tax on wage for {player.Name}"));
                    }

                    // --- СТЪПКА 3: КОМИСИОННА НА АГЕНТА (Играч -> Агенция) ---
                    var agentContract = player.RepresentationContracts.FirstOrDefault();
                    if (agentContract != null && agentContract.WageCommissionPercentage > 0)
                    {
                        decimal commissionAmount = Math.Round(wage * (agentContract.WageCommissionPercentage / 100m), 2);

                        if (player.Balance >= commissionAmount) // Уверяваме се, че играчът има пари след данъка
                        {
                            player.Balance -= commissionAmount;
                            var agency = agencies[agentContract.AgencyId];
                            agency.Budget += commissionAmount;
                            processedCommissions++;
                            transactionLogs.Add(CreateLog(EntityType.Player, player.Id, EntityType.Agency, agency.Id, commissionAmount, TransactionCategory.Commission, $"Commission from {player.Name}"));

                            // --- СТЪПКА 4: ДАНЪК ВЪРХУ КОМИСИОННАТА (Агенция -> Банка) ---
                            decimal commTax = Math.Round(commissionAmount * taxRate, 2);
                            if (commTax > 0)
                            {
                                agency.Budget -= commTax;
                                bank.ReserveBalance += commTax;
                                transactionLogs.Add(CreateLog(EntityType.Agency, agency.Id, EntityType.Bank, bank.Id, commTax, TransactionCategory.Tax, $"Income Tax on commission from {player.Name}"));
                            }
                        }
                    }
                }

                // ==============================================================
                // МАГИЯТА ЗА БЪРЗИНА СЕ СЛУЧВА ТУК!
                // ==============================================================

                // Добавяме всички 6000+ транзакции в EF Core наведнъж
                await _context.Transactions.AddRangeAsync(transactionLogs);

                // ЗАПАЗВАМЕ В БАЗАТА САМО ЕДИН ЕДИНСТВЕН ПЪТ ЗА ЦЯЛАТА СЕДМИЦА!
                await _context.SaveChangesAsync();

                await dbTransaction.CommitAsync();

                return (true, $"Успешно платени {processedWages} заплати. Събрани {processedCommissions} комисионни.");
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return (false, $"Грешка при масовото плащане на заплати: {ex.Message}");
            }
        }

        // Помощен метод за по-чист код (добави го в класа FinanceService)
        private Transaction CreateLog(EntityType sender, int senderId, EntityType receiver, int receiverId, decimal amount, TransactionCategory cat, string desc)
        {
            return new Transaction
            {
                Date = DateTime.UtcNow,
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