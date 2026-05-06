namespace TenPercent.Application.Services.Interfaces
{
    using System.Threading.Tasks;
    using TenPercent.Data.DTOs.Admin;

    public interface IAdminSettingsService
    {
        Task<EconomySettingsDto> GetSettingsAsync();
        Task<(bool Success, string Message)> UpdateSettingsAsync(EconomySettingsDto dto);
    }
}