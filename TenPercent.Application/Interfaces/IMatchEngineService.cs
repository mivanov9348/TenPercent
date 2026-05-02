namespace TenPercent.Application.Interfaces
{
    using TenPercent.Data.Models;

    public interface IMatchEngineService
    {
        Task<Fixture> PlayMatchAsync(Fixture match);
    }
}