namespace TenPercent.Application.Services.Interfaces
{
    using System.Threading.Tasks;
    using TenPercent.Api.DTOs;

    public interface IClubService
    {
        Task<ClubDetailsDto> GetClubDetailsAsync(int id);
    }
}