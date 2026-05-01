namespace TenPercent.Application.Services.Interfaces
{
    public interface ISeasonService
    {
        Task<(bool Success, string Message)> EndCurrentSeasonAsync();
        Task<(bool Success, string Message)> StartNewSeasonAsync();
    }
}