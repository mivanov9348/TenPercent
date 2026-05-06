namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.DTOs.Admin;
    using TenPercent.Data.Enums;

    public class AdminBankService : IAdminBankService
    {
        private readonly AppDbContext _context;

        public AdminBankService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BankDashboardDto> GetBankStatsAsync()
        {
            var bank = await _context.Banks.FirstOrDefaultAsync();
            if (bank == null) return new BankDashboardDto();

            // Изчисляваме колко пари има събрани от данъци
            var totalTaxes = await _context.Transactions
                .Where(t => t.Category == TransactionCategory.Tax)
                .SumAsync(t => t.Amount);

            // Изчисляваме колко пари банката е раздала на клубовете (Initial Allocation)
            var totalGrants = await _context.Transactions
                .Where(t => t.Category == TransactionCategory.InitialAllocation || t.Category == TransactionCategory.StartupGrant)
                .SumAsync(t => t.Amount);

            // Изчисляваме парите в обращение (Клубове + Агенции + Играчи)
            var clubMoney = await _context.Clubs.SumAsync(c => c.TransferBudget + c.WageBudget);
            var agencyMoney = await _context.Agencies.SumAsync(a => a.Budget);
            var playerMoney = await _context.Players.SumAsync(p => p.Balance);
            var moneyInCirculation = clubMoney + agencyMoney + playerMoney;

            // Взимаме последните 50 транзакции, които касаят банката
            var recentTxs = await _context.Transactions
                .Where(t => t.SenderType == EntityType.Bank || t.ReceiverType == EntityType.Bank)
                .OrderByDescending(t => t.Date)
                .Take(50)
                .ToListAsync();

            return new BankDashboardDto
            {
                ReserveBalance = bank.ReserveBalance,
                TotalTaxesCollected = totalTaxes,
                TotalGrantsGiven = totalGrants,
                MoneyInCirculation = moneyInCirculation,
                RecentTransactions = recentTxs
            };
        }
    }
}