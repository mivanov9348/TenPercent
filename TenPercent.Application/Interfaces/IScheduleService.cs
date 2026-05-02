namespace TenPercent.Application.Services.Interfaces
{
    using TenPercent.Data.Models;
    public interface IScheduleService
    {
        Task<(bool Success, string Message, List<Fixture> Fixtures)> GenerateLeagueScheduleAsync(int leagueId, int seasonId);
        Task<(bool Success, string Message, List<Fixture> Fixtures)> GenerateCupScheduleAsync(int seasonId);
        Task<(bool Success, string Message, List<Fixture> Fixtures)> GenerateEuropeanScheduleAsync(int seasonId);
    }
}