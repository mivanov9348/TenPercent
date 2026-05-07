namespace TenPercent.Api.Services
{
    using Bogus;
    using TenPercent.Data.Models;
    using System.Collections.Generic;
    using System;
    using TenPercent.Application.Interfaces;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Data;
    using Microsoft.EntityFrameworkCore;

    public class PlayerGeneratorService : IPlayerGeneratorService
    {
        private readonly AppDbContext _context;
        private readonly Random _rand = new Random();

        public PlayerGeneratorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message)> GenerateFreeAgentsAsync(int count)
        {
            var positions = await _context.Positions.ToListAsync();
            if (!positions.Any())
            {
                return (false, "Positions not imported. Please import positions first.");
            }

            var newPlayers = GenerateMultiplePlayers(count, "", null, positions);

            _context.Players.AddRange(newPlayers);
            await _context.SaveChangesAsync();

            return (true, $"Successfully generated {count} organic free agents into the world database.");
        }

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
                PositionId = position.Id,
                Position = position,
                ClubId = clubId,
                Form = "Good"
            };

            player.Age = GetRandomAge();

            // Задаваме целеви потенциал и текущо умение
            int targetPA = GetRandomAbility(70, 15);
            double ageFactor = Math.Clamp((player.Age - 15.0) / (28.0 - 15.0), 0.0, 1.0);
            int targetCA = (int)(40 + (targetPA - 40) * ageFactor);

            if (player.Age > 32)
            {
                targetCA = Math.Clamp(targetCA - _rand.Next(1, (player.Age - 30) * 2), 1, 100);
            }

            // 1. Генерираме атрибутите базирано на Позицията, Годините и Целевия OVR!
            player.Attributes = GenerateAttributes(player, targetCA, targetPA);

            // 2. След като атрибутите са готови, извикваме метода да сметне ИСТИНСКИЯ OVR!
            player.RecalculateCurrentAbility();

            // 3. Смятаме истинския потенциал
            player.PotentialAbility = CalculateOVR(player.Attributes, position, isPotential: true);

            // 4. Формула за цена
            decimal valueBase = player.CurrentAbility * 100000m;
            decimal potBonus = (player.PotentialAbility - player.CurrentAbility) * 50000m;
            decimal youthPremium = player.Age < 23 ? 1.5m : (player.Age > 30 ? 0.5m : 1.0m);

            player.MarketValue = (valueBase + potBonus) * youthPremium;

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

            // Обновяваме силата спрямо репутацията на клуба
            foreach (var p in squad)
            {
                int targetCA = Math.Clamp(clubReputation + _rand.Next(-10, 5), 40, 95);
                int targetPA = Math.Clamp(targetCA + _rand.Next(0, 15), targetCA, 99);

                p.Attributes = GenerateAttributes(p, targetCA, targetPA);
                p.RecalculateCurrentAbility(); // Перфектно калкулиран OVR
                p.PotentialAbility = CalculateOVR(p.Attributes, p.Position, isPotential: true);

                p.MarketValue = (p.CurrentAbility * 150000m);
            }

            return squad;
        }

        private PlayerAttributes GenerateAttributes(Player p, int targetCA, int targetPA)
        {
            var attr = new PlayerAttributes
            {
                Ambition = _rand.Next(1, 101),
                Greed = _rand.Next(1, 101),
                Loyalty = _rand.Next(1, 101),
                InjuryProne = _rand.Next(1, 100) // Генерираме склонност към контузии
            };

            var pos = p.Position;
            int age = p.Age;

            // --- УМНАТА ФОРМУЛА ЗА ГЕНЕРИРАНЕ ---
            int GenStat(int targetOvr, decimal weight, bool isPhysical)
            {
                // База: Половината от таргета + тежестта на атрибута * мултипликатор
                int val = (int)(targetOvr * 0.5m + (targetOvr * 1.8m * weight) + _rand.Next(-6, 7));

                // ВЛИЯНИЕ НА ГОДИНИТЕ:
                if (age > 30)
                {
                    if (isPhysical) val -= (age - 30) * 2; // Физиката пада рязко
                    else val += (age - 30); // Менталните се вдигат леко (Опит)
                }
                else if (age < 21)
                {
                    if (!isPhysical) val -= (21 - age) * 2; // Младите грешат и нямат поглед
                    else val += (21 - age); // Но имат неизчерпаема енергия
                }

                return Math.Clamp(val, 1, 99);
            }

            // Генериране на ТЕКУЩИ атрибути (чрез използване на тежестите от Position!)
            attr.Pace = GenStat(targetCA, pos.PaceWeight, isPhysical: true);
            attr.Shooting = GenStat(targetCA, pos.ShootingWeight, isPhysical: false);
            attr.Passing = GenStat(targetCA, pos.PassingWeight, isPhysical: false);
            attr.Dribbling = GenStat(targetCA, pos.DribblingWeight, isPhysical: false);
            attr.Defending = GenStat(targetCA, pos.DefendingWeight, isPhysical: false);
            attr.Physical = GenStat(targetCA, pos.PhysicalWeight, isPhysical: true);

            attr.Goalkeeping = GenStat(targetCA, pos.GoalkeepingWeight, isPhysical: false);
            attr.Vision = GenStat(targetCA, pos.VisionWeight, isPhysical: false);
            attr.Stamina = GenStat(targetCA, pos.StaminaWeight, isPhysical: true);

            // Генериране на ПОТЕНЦИАЛНИ атрибути (трябва да са поне колкото текущите)
            attr.PotentialPace = Math.Max(attr.Pace, GenStat(targetPA, pos.PaceWeight, true));
            attr.PotentialShooting = Math.Max(attr.Shooting, GenStat(targetPA, pos.ShootingWeight, false));
            attr.PotentialPassing = Math.Max(attr.Passing, GenStat(targetPA, pos.PassingWeight, false));
            attr.PotentialDribbling = Math.Max(attr.Dribbling, GenStat(targetPA, pos.DribblingWeight, false));
            attr.PotentialDefending = Math.Max(attr.Defending, GenStat(targetPA, pos.DefendingWeight, false));
            attr.PotentialPhysical = Math.Max(attr.Physical, GenStat(targetPA, pos.PhysicalWeight, true));

            attr.PotentialGoalkeeping = Math.Max(attr.Goalkeeping, GenStat(targetPA, pos.GoalkeepingWeight, false));
            attr.PotentialVision = Math.Max(attr.Vision, GenStat(targetPA, pos.VisionWeight, false));
            attr.PotentialStamina = Math.Max(attr.Stamina, GenStat(targetPA, pos.StaminaWeight, true));

            return attr;
        }

        // Помощен метод за изчисляване на Потенциала
        private int CalculateOVR(PlayerAttributes attr, Position pos, bool isPotential)
        {
            decimal ovr =
                ((isPotential ? attr.PotentialPace : attr.Pace) * pos.PaceWeight) +
                ((isPotential ? attr.PotentialShooting : attr.Shooting) * pos.ShootingWeight) +
                ((isPotential ? attr.PotentialPassing : attr.Passing) * pos.PassingWeight) +
                ((isPotential ? attr.PotentialDribbling : attr.Dribbling) * pos.DribblingWeight) +
                ((isPotential ? attr.PotentialDefending : attr.Defending) * pos.DefendingWeight) +
                ((isPotential ? attr.PotentialPhysical : attr.Physical) * pos.PhysicalWeight) +
                ((isPotential ? attr.PotentialGoalkeeping : attr.Goalkeeping) * pos.GoalkeepingWeight) +
                ((isPotential ? attr.PotentialVision : attr.Vision) * pos.VisionWeight) +
                ((isPotential ? attr.PotentialStamina : attr.Stamina) * pos.StaminaWeight);

            return (int)Math.Clamp(Math.Round(ovr, 0), 1, 99);
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