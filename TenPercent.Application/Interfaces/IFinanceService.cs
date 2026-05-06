namespace TenPercent.Application.Services.Interfaces
{
    using System.Threading.Tasks;
    using TenPercent.Data.Enums;

    public interface IFinanceService
    {
        Task<(bool Success, string Message)> ProcessTransactionAsync(
            EntityType senderType, int? senderId,
            EntityType receiverType, int? receiverId,
            decimal amount, TransactionCategory category, string description);
        Task<(bool Success, string Message)> InitializeWorldEconomyAsync(decimal initialBankBudget);

        Task<(bool Success, string Message)> ProcessWeeklyWagesAsync();
    }
}