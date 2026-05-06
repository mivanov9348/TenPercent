namespace TenPercent.Application.Services.Interfaces
{
    using System.Threading.Tasks;
    using TenPercent.Data.DTOs.Admin;
    public interface IAdminBankService
    {
        Task<BankDashboardDto> GetBankStatsAsync();
    }
}