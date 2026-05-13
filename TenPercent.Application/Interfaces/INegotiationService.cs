namespace TenPercent.Application.Services.Interfaces
{
    using System.Threading.Tasks;
    using TenPercent.Application.DTOs;

    public interface INegotiationService
    {
        Task<ContractResponseDto> ProposeContractAsync(int userId, ContractOfferDto offer);
        Task<ContractResponseDto> RenewContractAsync(int userId, ContractOfferDto offer);
        Task<(bool Success, string Message)> SendPoachOfferAsync(int sendingUserId, AgencyPoachOfferDto offerDto);
        Task<(bool Success, string Message)> RespondToPoachOfferAsync(int respondingUserId, RespondToMessageOfferDto responseDto);
    }
}