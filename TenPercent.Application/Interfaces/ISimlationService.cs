namespace TenPercent.Application.Interfaces
{
    using TenPercent.Data.Models;

    public interface ISimulationService
    {
        Task<(bool Success, string Message)> SimulateNextGameweekAsync();
    }
}
