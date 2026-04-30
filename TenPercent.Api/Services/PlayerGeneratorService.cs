namespace TenPercent.Api.Services
{
    using Bogus;
    using TenPercent.Api.Services.Interfaces;
    using TenPercent.Data.Models;

    public class PlayerGeneratorService : IPlayerGeneratorService
    {
        private readonly Random _rand = new Random();
        private readonly string[] _positions = { "ST", "MID", "DEF", "GK" };

        public List<Player> GenerateMultiplePlayers(int count, string type, int? clubId, string? position = null)
        {
            var players = new List<Player>();
            for (int i = 0; i < count; i++)
            {
                players.Add(GeneratePlayer(type, clubId, position));
            }
            return players;
        }

        // ТУК Е ПОПРАВКАТА: Добавен е третият параметър string? position = null
        public Player GeneratePlayer(string type, int? clubId, string? position = null)
        {
            var faker = new Faker("en");

            var player = new Player
            {
                Name = $"{faker.Name.FirstName(Bogus.DataSets.Name.Gender.Male)} {faker.Name.LastName()}",
                Nationality = faker.Address.Country(),

                // ТУК Е ПОПРАВКАТА: Ако сме подали позиция, ползваме нея. Ако не, избираме рандом.
                Position = position ?? _positions[_rand.Next(_positions.Length)],

                ClubId = clubId,
                Form = "Good"
            };

            if (type == "Wonderkid")
            {
                player.Age = _rand.Next(15, 19);
                player.Overall = _rand.Next(60, 75);
                player.Potential = _rand.Next(85, 95);
            }
            else if (type == "Veteran")
            {
                player.Age = _rand.Next(31, 38);
                player.Overall = _rand.Next(75, 85);
                player.Potential = player.Overall;
            }
            else // Normal
            {
                player.Age = _rand.Next(19, 30);
                player.Overall = _rand.Next(65, 82);
                player.Potential = player.Overall + _rand.Next(0, 8);
            }

            GenerateStatsForPosition(player);

            // Финанси
            decimal ageFactor = player.Age < 22 ? 1.5m : (player.Age > 30 ? 0.5m : 1.0m);
            player.MarketValue = (player.Overall * 100000) * ageFactor;

            player.ContractYearsLeft = clubId.HasValue ? _rand.Next(1, 6) : 0;
            player.WeeklyWage = clubId.HasValue ? (player.Overall * 800) : 0;

            return player;
        }


        private void GenerateStatsForPosition(Player p)
        {
            int GenStat(int baseStat) => Math.Clamp(baseStat + _rand.Next(-5, 6), 1, 99);

            if (p.Position == "ST")
            {
                p.Pace = GenStat(p.Overall + 5);
                p.Shooting = GenStat(p.Overall + 8);
                p.Passing = GenStat(p.Overall - 5);
                p.Dribbling = GenStat(p.Overall + 2);
                p.Defending = GenStat(p.Overall - 20);
                p.Physical = GenStat(p.Overall);
            }
            else if (p.Position == "MID")
            {
                p.Pace = GenStat(p.Overall);
                p.Shooting = GenStat(p.Overall);
                p.Passing = GenStat(p.Overall + 8);
                p.Dribbling = GenStat(p.Overall + 5);
                p.Defending = GenStat(p.Overall - 5);
                p.Physical = GenStat(p.Overall - 2);
            }
            else
            {
                p.Pace = GenStat(p.Overall - 5);
                p.Shooting = GenStat(p.Overall - 20);
                p.Passing = GenStat(p.Overall - 5);
                p.Dribbling = GenStat(p.Overall - 10);
                p.Defending = GenStat(p.Overall + 10);
                p.Physical = GenStat(p.Overall + 8);
            }
        }
    }
}