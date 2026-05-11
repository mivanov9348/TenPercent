namespace TenPercent.Application.Services.Interfaces
{
    using System.Threading.Tasks;
    using TenPercent.Api.DTOs;

    public interface IPlayerContractService
    {
        Task<bool> GenerateInitialContractsAsync();

        Task<(bool Success, string Message, bool Accepted)> NegotiateContractAsync(NegotiateClubContractDto dto);
        Task ProcessContractsYearEndAsync(int endingSeasonNumber);
    }
}