namespace TenPercent.Application.Services.Interfaces
{
    using System.Threading.Tasks;
    using TenPercent.Api.DTOs; // Или където си пазиш DTO-тата за репорта

    public interface IScoutingService
    {
        Task<(bool Success, string Message, object Report)> GeneratePaidReportAsync(int userId, int playerId);
        Task<object?> GetReportAsync(int userId, int playerId);
    }
}