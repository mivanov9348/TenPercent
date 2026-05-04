namespace TenPercent.Application.Interfaces
{
    using System.Threading.Tasks;
    using TenPercent.Api.DTOs;

    public interface IPlayerService
    {
        Task<PlayerDetailsDto> GetPlayerDetailsAsync(int id);
        Task<(bool Success, string Message)> GenerateFreeAgentsAsync(int count);
        Task<PaginatedResultDto<ScoutingPlayerDto>> GetScoutingPoolAsync(
            string? search, string? position, string? nationality,
            int? minAge, int? maxAge, decimal? maxValue,
            bool? hasAgency, string? sortBy, int page, int pageSize);
    }
}