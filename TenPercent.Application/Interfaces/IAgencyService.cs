namespace TenPercent.Application.Services.Interfaces
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using TenPercent.Api.DTOs;
    using TenPercent.Application.DTOs;

    public interface IAgencyService
    {
        Task<AgencyDto> GetMyAgencyAsync(int userId);
        Task<List<AgencyPlayerDto>> GetAgencyPlayersAsync(int userId);
        Task<(bool Success, string Message, bool Accepted)> OfferRepresentationAsync(int userId, OfferRepresentationDto dto);
        Task<(bool Success, string Message)> CreateAgencyAsync(CreateAgencyDto dto);
        Task<AgencyFinanceDto?> GetAgencyFinanceAsync(int userId);
    }
}