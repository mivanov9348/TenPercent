namespace TenPercent.Application.Services.Interfaces
{
    using System.Threading.Tasks;
    using TenPercent.Data.Models;

    public interface IScoutReportGenerator
    {
        Task<ScoutReport> GenerateReportAsync(ScoutReport report, Player player, int knowledgeLevel);
    }
}