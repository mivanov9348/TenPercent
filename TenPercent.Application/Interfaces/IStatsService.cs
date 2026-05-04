namespace TenPercent.Application.Services.Interfaces
{
    using System.Threading.Tasks;
    using TenPercent.Api.DTOs;

    public interface IStatsService
    {
        Task<SeasonStatsDto> GetCurrentSeasonStatsAsync();
    }
}