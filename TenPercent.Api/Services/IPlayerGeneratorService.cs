namespace TenPercent.Api.Services.Interfaces
{
    using TenPercent.Data.Models;

    public interface IPlayerGeneratorService
    {
        Player GeneratePlayer(string tier, int? clubId, string? position = null);

        List<Player> GenerateMultiplePlayers(int count, string tier, int? clubId, string? position = null);

        List<Player> GenerateFullSquadForClub(int clubId, int clubReputation);
    }
}