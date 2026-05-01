namespace TenPercent.Api.Services
{
    using Bogus;
    using TenPercent.Api.Services.Interfaces;
    using TenPercent.Data.Models;
    using System.Collections.Generic;
    using System;

    public class PlayerGeneratorService : IPlayerGeneratorService
    {
        private readonly Random _rand = new Random();
        private readonly string[] _positions = { "ST", "MID", "DEF", "GK" };

        public List<Player> GenerateMultiplePlayers(int count, string tier, int? clubId, string? position = null)
        {
            var players = new List<Player>();
            for (int i = 0; i < count; i++)
            {
                players.Add(GeneratePlayer(tier, clubId, position));
            }
            return players;
        }

        // Забележка: 'tier' параметърът вече се игнорира, тъй като всичко е органично,
        // но го оставяме в подписа, за да не чупим интерфейса веднага.
        public Player GeneratePlayer(string tier, int? clubId, string? position = null)
        {
            var faker = new Faker("en");

            var player = new Player
            {
                Name = $"{faker.Name.FirstName(Bogus.DataSets.Name.Gender.Male)} {faker.Name.LastName()}",
                Nationality = faker.Address.Country(),
                Position = position ?? _positions[_rand.Next(_positions.Length)],
                ClubId = clubId,
                Form = "Good"
            };

            // 1. ГЕНЕРИРАНЕ НА ВЪЗРАСТ (Нормално разпределение между 15 и 36)
            player.Age = GetRandomAge();

            // 2. ГЕНЕРИРАНЕ НА ПОТЕНЦИАЛ (PA: 1-100)
            // Повечето играчи са около 65-75. Много рядко има 90+ (Гаусова крива).
            player.PotentialAbility = GetRandomAbility(70, 15);

            // 3. ГЕНЕРИРАНЕ НА ТЕКУЩО НИВО (CA: 1-100)
            // Колкото по-възрастен е играчът, толкова по-близо е CA до PA.
            double ageFactor = Math.Clamp((player.Age - 15.0) / (28.0 - 15.0), 0.0, 1.0);
            // Ако е на 28 или повече, CA е почти равно на PA (или е почнал да регресира).
            int currentAbilityBase = (int)(40 + (player.PotentialAbility - 40) * ageFactor);

            // Добавяме лек ранъм елемент, за да има "провалили се таланти" на 25 г.
            player.CurrentAbility = Math.Clamp(currentAbilityBase + _rand.Next(-5, 5), 1, player.PotentialAbility);

            if (player.Age > 32)
            {
                // Регресия за ветераните
                player.CurrentAbility = Math.Clamp(player.CurrentAbility - _rand.Next(1, (player.Age - 30) * 2), 1, 100);
            }

            // 4. СЪЗДАВАНЕ НА АТРИБУТИТЕ
            player.Attributes = GenerateAttributes(player);

            // 5. ФИНАНСИ И ДОГОВОР
            // Стойността зависи от CA, PA и възрастта
            decimal valueBase = player.CurrentAbility * 100000m;
            decimal potBonus = (player.PotentialAbility - player.CurrentAbility) * 50000m;
            decimal youthPremium = player.Age < 23 ? 1.5m : (player.Age > 30 ? 0.5m : 1.0m);

            player.MarketValue = (valueBase + potBonus) * youthPremium;

            player.ContractYearsLeft = clubId.HasValue ? _rand.Next(1, 6) : 0;
            // Заплатата зависи от CA и Greed
            decimal greedFactor = 1.0m + (player.Attributes.Greed - 50m) / 100m; // Greed 100 = 50% по-висока заплата
            player.WeeklyWage = clubId.HasValue ? (player.CurrentAbility * 1000m) * greedFactor : 0;

            return player;
        }

        public List<Player> GenerateFullSquadForClub(int clubId, int clubReputation)
        {
            var squad = new List<Player>();

            // 1 Вратар (GK)
            squad.Add(GeneratePlayer("", clubId, "GK"));

            // 4 Защитника (DEF)
            for (int i = 0; i < 4; i++) squad.Add(GeneratePlayer("", clubId, "DEF"));

            // 4 Халфа (MID)
            for (int i = 0; i < 4; i++) squad.Add(GeneratePlayer("", clubId, "MID"));

            // 2 Нападателя (ST)
            for (int i = 0; i < 2; i++) squad.Add(GeneratePlayer("", clubId, "ST"));

            // 5 Резерви 
            squad.Add(GeneratePlayer("", clubId, "GK"));
            squad.Add(GeneratePlayer("", clubId, "DEF"));
            squad.Add(GeneratePlayer("", clubId, "MID"));
            squad.Add(GeneratePlayer("", clubId, "MID"));
            squad.Add(GeneratePlayer("", clubId, "ST"));

            // Нагласяме CA/PA спрямо репутацията на клуба
            foreach (var p in squad)
            {
                // Клубове с репутация 90 имат играчи с CA около 80-90
                int targetCA = Math.Clamp(clubReputation + _rand.Next(-10, 5), 40, 95);
                p.CurrentAbility = targetCA;
                p.PotentialAbility = Math.Clamp(targetCA + _rand.Next(0, 15), targetCA, 99);

                // Прегенерираме атрибутите спрямо новия CA
                p.Attributes = GenerateAttributes(p);

                p.MarketValue = (p.CurrentAbility * 150000m);
                p.WeeklyWage = (p.CurrentAbility * 1000m);
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

            if (p.Position == "ST")
            {
                attr.Pace = GenStat(p.CurrentAbility + 5);
                attr.Shooting = GenStat(p.CurrentAbility + 15);
                attr.Passing = GenStat(p.CurrentAbility - 10);
                attr.Dribbling = GenStat(p.CurrentAbility);
                attr.Defending = GenStat(p.CurrentAbility - 30);
                attr.Physical = GenStat(p.CurrentAbility);

                attr.PotentialPace = GenStat(p.PotentialAbility + 5);
                attr.PotentialShooting = GenStat(p.PotentialAbility + 15);
                attr.PotentialPassing = GenStat(p.PotentialAbility - 10);
                attr.PotentialDribbling = GenStat(p.PotentialAbility);
                attr.PotentialDefending = GenStat(p.PotentialAbility - 30);
                attr.PotentialPhysical = GenStat(p.PotentialAbility);
            }
            else if (p.Position == "MID")
            {
                attr.Pace = GenStat(p.CurrentAbility);
                attr.Shooting = GenStat(p.CurrentAbility - 5);
                attr.Passing = GenStat(p.CurrentAbility + 15);
                attr.Dribbling = GenStat(p.CurrentAbility + 10);
                attr.Defending = GenStat(p.CurrentAbility - 5);
                attr.Physical = GenStat(p.CurrentAbility - 5);

                attr.PotentialPace = GenStat(p.PotentialAbility);
                attr.PotentialShooting = GenStat(p.PotentialAbility - 5);
                attr.PotentialPassing = GenStat(p.PotentialAbility + 15);
                attr.PotentialDribbling = GenStat(p.PotentialAbility + 10);
                attr.PotentialDefending = GenStat(p.PotentialAbility - 5);
                attr.PotentialPhysical = GenStat(p.PotentialAbility - 5);
            }
            else if (p.Position == "DEF")
            {
                attr.Pace = GenStat(p.CurrentAbility - 5);
                attr.Shooting = GenStat(p.CurrentAbility - 30);
                attr.Passing = GenStat(p.CurrentAbility - 5);
                attr.Dribbling = GenStat(p.CurrentAbility - 10);
                attr.Defending = GenStat(p.CurrentAbility + 15);
                attr.Physical = GenStat(p.CurrentAbility + 10);

                attr.PotentialPace = GenStat(p.PotentialAbility - 5);
                attr.PotentialShooting = GenStat(p.PotentialAbility - 30);
                attr.PotentialPassing = GenStat(p.PotentialAbility - 5);
                attr.PotentialDribbling = GenStat(p.PotentialAbility - 10);
                attr.PotentialDefending = GenStat(p.PotentialAbility + 15);
                attr.PotentialPhysical = GenStat(p.PotentialAbility + 10);
            }
            else // GK
            {
                attr.Pace = GenStat(p.CurrentAbility - 40);
                attr.Shooting = GenStat(p.CurrentAbility - 40);
                attr.Passing = GenStat(p.CurrentAbility - 15);
                attr.Dribbling = GenStat(p.CurrentAbility - 30);
                attr.Defending = GenStat(p.CurrentAbility + 20); // Рефлекси за вратар
                attr.Physical = GenStat(p.CurrentAbility + 10);

                attr.PotentialPace = GenStat(p.PotentialAbility - 40);
                attr.PotentialShooting = GenStat(p.PotentialAbility - 40);
                attr.PotentialPassing = GenStat(p.PotentialAbility - 15);
                attr.PotentialDribbling = GenStat(p.PotentialAbility - 30);
                attr.PotentialDefending = GenStat(p.PotentialAbility + 20);
                attr.PotentialPhysical = GenStat(p.PotentialAbility + 10);
            }

            // Предпазна мярка - потенциалът не може да е по-нисък от текущия атрибут
            attr.PotentialPace = Math.Max(attr.Pace, attr.PotentialPace);
            attr.PotentialShooting = Math.Max(attr.Shooting, attr.PotentialShooting);
            attr.PotentialPassing = Math.Max(attr.Passing, attr.PotentialPassing);
            attr.PotentialDribbling = Math.Max(attr.Dribbling, attr.PotentialDribbling);
            attr.PotentialDefending = Math.Max(attr.Defending, attr.PotentialDefending);
            attr.PotentialPhysical = Math.Max(attr.Physical, attr.PotentialPhysical);

            return attr;
        }

        // Помощни функции за нормално разпределение (Гаусова крива)
        private int GetRandomAge()
        {
            // Повечето играчи са около 24г.
            double u1 = 1.0 - _rand.NextDouble();
            double u2 = 1.0 - _rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            double randNormal = 24 + 4 * randStdNormal; // Средна възраст 24, отклонение 4
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