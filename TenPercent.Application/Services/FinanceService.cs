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

            // ==========================================
            // НОВО: ПРОВЕРКА ЗА АКТИВНА ТРАНЗАКЦИЯ
            // ==========================================
            bool isNestedTransaction = _context.Database.CurrentTransaction != null;

            // Започваме нова само ако няма вече съществуваща такава
            var dbTransaction = isNestedTransaction ? null : await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. ВАДИМ ПАРИ ОТ ИЗПРАЩАЧА
                var senderResult = await UpdateEntityBalance(senderType, senderId, -amount);
                if (!senderResult.Success) return senderResult;

                // 2. ДАВАМЕ ПАРИ НА ПОЛУЧАТЕЛЯ
                var receiverResult = await UpdateEntityBalance(receiverType, receiverId, amount);
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

                // 4. АВТОМАТИЧЕН ДАНЪК
                if (receiverType == EntityType.Agency && senderType != EntityType.Bank && category == TransactionCategory.Commission)
                {
                    await ApplyAgencyIncomeTaxAsync(receiverId.Value, amount, transactionLog.Id);
                }

                // ==========================================
                // НОВО: КОМИТВАМЕ САМО АКО НИЕ СМЕ Я СЪЗДАЛИ
                // ==========================================
                if (dbTransaction != null)
                {
                    await dbTransaction.CommitAsync();
                }

                return (true, "Транзакцията е успешна.");
            }
            catch (Exception ex)
            {
                // РОЛБЕК САМО АКО НИЕ СМЕ Я СЪЗДАЛИ
                if (dbTransaction != null)
                {
                    await dbTransaction.RollbackAsync();
                }
                return (false, "Грешка при счетоводната транзакция: " + ex.Message);
            }
            finally
            {
                // ВИНАГИ РАЗЧИСТВАМЕ РЕСУРСИТЕ
                if (dbTransaction != null)
                {
                    await dbTransaction.DisposeAsync();
                }
            }
        }

        // ПОМОЩЕН МЕТОД ЗА ДАНЪКА
        private async Task ApplyAgencyIncomeTaxAsync(int agencyId, decimal incomeAmount, int originalTxId)
        {
            var settings = await _context.EconomySettings.FirstOrDefaultAsync();
            decimal taxRate = settings?.AgencyIncomeTaxRate ?? 0.10m;

            if (taxRate <= 0) return;

            decimal taxAmount = Math.Round(incomeAmount * taxRate, 2);
            var bank = await _context.Banks.FirstOrDefaultAsync();
            var agency = await _context.Agencies.FindAsync(agencyId);

            if (agency != null && bank != null && taxAmount > 0)
            {
                // Извършваме превода към банката
                agency.Budget -= taxAmount;
                bank.ReserveBalance += taxAmount;

                // Логваме данъчната транзакция
                var taxLog = new Transaction
                {
                    Date = DateTime.UtcNow,
                    Amount = taxAmount,
                    SenderType = EntityType.Agency,
                    SenderId = agencyId,
                    ReceiverType = EntityType.Bank,
                    ReceiverId = bank.Id,
                    Category = TransactionCategory.Tax,
                    Description = $"Income Tax ({taxRate * 100}%) on revenue from Tx #{originalTxId}"
                };
                _context.Transactions.Add(taxLog);
                await _context.SaveChangesAsync();
            }
        }

        // ПОМОЩЕН МЕТОД ЗА ОБНОВЯВАНЕ НА БАЛАНСИ
        private async Task<(bool Success, string Message)> UpdateEntityBalance(EntityType type, int? id, decimal amount)
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
                if (amount < 0 && club.TransferBudget < Math.Abs(amount))
                    return (false, $"Недостатъчни средства в клуб {club.Name}.");
                club.TransferBudget += amount;
            }
            else if (type == EntityType.Agency && id.HasValue)
            {
                var agency = await _context.Agencies.FindAsync(id.Value);
                if (agency == null) return (false, "Агенцията не е намерена.");
                if (amount < 0 && agency.Budget < Math.Abs(amount))
                    return (false, "Недостатъчен бюджет на агенцията.");
                agency.Budget += amount;
            }
            // Играчите (EntityType.Player) само приемат пари (излизат от икономиката)

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
    }
}