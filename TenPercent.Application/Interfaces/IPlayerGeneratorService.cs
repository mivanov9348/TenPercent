namespace TenPercent.Application.Interfaces
{
    using System.Collections.Generic;
    using TenPercent.Data.Models;

    public interface IPlayerGeneratorService
    {
        Player GeneratePlayer(string tier, int? clubId, Position position);

        List<Player> GenerateMultiplePlayers(int count, string tier, int? clubId, List<Position> availablePositions, Position? specificPosition = null);

        List<Player> GenerateFullSquadForClub(int clubId, int clubReputation, List<Position> availablePositions);
    }
}