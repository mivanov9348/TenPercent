namespace TenPercent.Api.Services
{
    using Bogus;
    using TenPercent.Data.Models;
    using System.Collections.Generic;
    using System;
    using TenPercent.Application.Interfaces;
    using System.Linq;

    public class PlayerGeneratorService : IPlayerGeneratorService
    {
        private readonly Random _rand = new Random();

        public List<Player> GenerateMultiplePlayers(int count, string tier, int? clubId, List<Position> availablePositions, Position? specificPosition = null)
        {
            var players = new List<Player>();
            for (int i = 0; i < count; i++)
            {
                var posToAssign = specificPosition ?? availablePositions[_rand.Next(availablePositions.Count)];
                players.Add(GeneratePlayer(tier, clubId, posToAssign));
            }
            return players;
        }

        public Player GeneratePlayer(string tier, int? clubId, Position position)
        {
            var faker = new Faker("en");

            var player = new Player
            {
                Name = $"{faker.Name.FirstName(Bogus.DataSets.Name.Gender.Male)} {faker.Name.LastName()}",
                Nationality = faker.Address.Country(),
                PositionId = position.Id, // Записваме ID-то за базата
                Position = position,      // Записваме обекта, за да го ползваме в GenerateAttributes
                ClubId = clubId,
                Form = "Good"
            };

            player.Age = GetRandomAge();
            player.PotentialAbility = GetRandomAbility(70, 15);

            double ageFactor = Math.Clamp((player.Age - 15.0) / (28.0 - 15.0), 0.0, 1.0);
            int currentAbilityBase = (int)(40 + (player.PotentialAbility - 40) * ageFactor);

            player.CurrentAbility = Math.Clamp(currentAbilityBase + _rand.Next(-5, 5), 1, player.PotentialAbility);

            if (player.Age > 32)
            {
                player.CurrentAbility = Math.Clamp(player.CurrentAbility - _rand.Next(1, (player.Age - 30) * 2), 1, 100);
            }

            player.Attributes = GenerateAttributes(player);

            decimal valueBase = player.CurrentAbility * 100000m;
            decimal potBonus = (player.PotentialAbility - player.CurrentAbility) * 50000m;
            decimal youthPremium = player.Age < 23 ? 1.5m : (player.Age > 30 ? 0.5m : 1.0m);

            player.MarketValue = (valueBase + potBonus) * youthPremium;

            decimal greedFactor = 1.0m + (player.Attributes.Greed - 50m) / 100m;

            return player;
        }

        public List<Player> GenerateFullSquadForClub(int clubId, int clubReputation, List<Position> availablePositions)
        {
            var squad = new List<Player>();

            var posGk = availablePositions.First(p => p.Abbreviation == "GK");
            var posCb = availablePositions.First(p => p.Abbreviation == "DEF");
            var posCm = availablePositions.First(p => p.Abbreviation == "MID");
            var posSt = availablePositions.First(p => p.Abbreviation == "ST");

            squad.Add(GeneratePlayer("", clubId, posGk));
            for (int i = 0; i < 4; i++) squad.Add(GeneratePlayer("", clubId, posCb));
            for (int i = 0; i < 4; i++) squad.Add(GeneratePlayer("", clubId, posCm));
            for (int i = 0; i < 2; i++) squad.Add(GeneratePlayer("", clubId, posSt));

            squad.Add(GeneratePlayer("", clubId, posGk));
            squad.Add(GeneratePlayer("", clubId, posCb));
            squad.Add(GeneratePlayer("", clubId, posCm));
            squad.Add(GeneratePlayer("", clubId, posCm));
            squad.Add(GeneratePlayer("", clubId, posSt));

            foreach (var p in squad)
            {
                int targetCA = Math.Clamp(clubReputation + _rand.Next(-10, 5), 40, 95);
                p.CurrentAbility = targetCA;
                p.PotentialAbility = Math.Clamp(targetCA + _rand.Next(0, 15), targetCA, 99);

                p.Attributes = GenerateAttributes(p);
                p.MarketValue = (p.CurrentAbility * 150000m);
            }

            return squad;
        }

        private PlayerAttributes GenerateAttributes(Player p)
        {
            var attr = new PlayerAttributes
            {
                Ambition = _rand.Next(1, 101),
                Greed = _rand.Next(1, 101),
                Loyalty = _rand.Next(1, 101)
            };

            int GenStat(int baseAbility) => Math.Clamp(baseAbility + _rand.Next(-10, 11), 1, 100);

            // ТУК Е ВАЖНАТА ПРОМЯНА: Сравняваме с Abbreviation (ST, CM, CB, GK)
            if (p.Position.Abbreviation == "ST")
            {
                attr.Pace = GenStat(p.CurrentAbility + 5);
                attr.Shooting = GenStat(p.CurrentAbility + 15);
                attr.Passing = GenStat(p.CurrentAbility - 10);
                attr.Dribbling = GenStat(p.CurrentAbility);
                attr.Defending = GenStat(p.CurrentAbility - 30);
                attr.Physical = GenStat(p.CurrentAbility);
            }
            else if (p.Position.Abbreviation == "MID")
            {
                attr.Pace = GenStat(p.CurrentAbility);
                attr.Shooting = GenStat(p.CurrentAbility - 5);
                attr.Passing = GenStat(p.CurrentAbility + 15);
                attr.Dribbling = GenStat(p.CurrentAbility + 10);
                attr.Defending = GenStat(p.CurrentAbility - 5);
                attr.Physical = GenStat(p.CurrentAbility - 5);
            }
            else if (p.Position.Abbreviation == "DEF")
            {
                attr.Pace = GenStat(p.CurrentAbility - 5);
                attr.Shooting = GenStat(p.CurrentAbility - 30);
                attr.Passing = GenStat(p.CurrentAbility - 5);
                attr.Dribbling = GenStat(p.CurrentAbility - 10);
                attr.Defending = GenStat(p.CurrentAbility + 15);
                attr.Physical = GenStat(p.CurrentAbility + 10);
            }
            else // GK
            {
                attr.Pace = GenStat(p.CurrentAbility - 40);
                attr.Shooting = GenStat(p.CurrentAbility - 40);
                attr.Passing = GenStat(p.CurrentAbility - 15);
                attr.Dribbling = GenStat(p.CurrentAbility - 30);
                attr.Defending = GenStat(p.CurrentAbility + 20);
                attr.Physical = GenStat(p.CurrentAbility + 10);
            }

            // Потенциалите
            attr.PotentialPace = Math.Max(attr.Pace, GenStat(p.PotentialAbility));
            attr.PotentialShooting = Math.Max(attr.Shooting, GenStat(p.PotentialAbility));
            attr.PotentialPassing = Math.Max(attr.Passing, GenStat(p.PotentialAbility));
            attr.PotentialDribbling = Math.Max(attr.Dribbling, GenStat(p.PotentialAbility));
            attr.PotentialDefending = Math.Max(attr.Defending, GenStat(p.PotentialAbility));
            attr.PotentialPhysical = Math.Max(attr.Physical, GenStat(p.PotentialAbility));

            return attr;
        }

        private int GetRandomAge()
        {
            double u1 = 1.0 - _rand.NextDouble();
            double u2 = 1.0 - _rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            double randNormal = 24 + 4 * randStdNormal;
            return Math.Clamp((int)Math.Round(randNormal), 15, 38);
        }

        private int GetRandomAbility(int mean, int stdDev)
        {
            double u1 = 1.0 - _rand.NextDouble();
            double u2 = 1.0 - _rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            double randNormal = mean + stdDev * randStdNormal;
            return Math.Clamp((int)Math.Round(randNormal), 30, 99);
        }
    }
}