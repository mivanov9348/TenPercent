namespace TenPercent.Api.Services.Interfaces
{
    using TenPercent.Data.Models;
    public interface IPlayerGeneratorService
    {
        Player GeneratePlayer(string type, int? clubId);
        List<Player> GenerateMultiplePlayers(int count, string type, int? clubId);
    }
}