namespace TenPercent.Application.Services.Interfaces
{
    public interface ILeagueService
    {
        Task<object> GetLiveStandingsAsync();
    }
}   