namespace TenPercent.Application.Services.Interfaces
{
    public interface IFixtureService
    {
        Task<object?> GetFixturesByLeagueAndGameweekAsync();
    }
}