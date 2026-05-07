namespace TenPercent.Application.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Application.Interfaces;
    using TenPercent.Data.Models;

    public class MatchEngineService : IMatchEngineService
    {
        private readonly Random _rand = new Random();

        public Task<Fixture> PlayMatchAsync(Fixture match)
        {
            var homePlayers = match.HomeClub.Players.ToList();
            var awayPlayers = match.AwayClub.Players.ToList();

            // Инициализираме статистиките
            InitializePerformances(match, homePlayers);
            InitializePerformances(match, awayPlayers);

            if (!homePlayers.Any() || !awayPlayers.Any())
            {
                match.IsPlayed = true;
                return Task.FromResult(match);
            }

            // --- СИМУЛАЦИЯ 1 до 90 МИНУТА ---
            for (int minute = 1; minute <= 90; minute++)
            {
                var activeHome = match.Performances.Where(p => p.FixtureId == match.Id && homePlayers.Any(hp => hp.Id == p.PlayerId) && p.RedCards == 0 && p.InjuryDays == 0).ToList();
                var activeAway = match.Performances.Where(p => p.FixtureId == match.Id && awayPlayers.Any(ap => ap.Id == p.PlayerId) && p.RedCards == 0 && p.InjuryDays == 0).ToList();

                // НОВО: Изчисляваме умората базирано на Stamina
                double homeFatigue = CalculateFatigue(activeHome, homePlayers, minute);
                double awayFatigue = CalculateFatigue(activeAway, awayPlayers, minute);

                // Атаката и защитата се влияят от умората!
                double homeAttack = CalculateAttackPower(activeHome, homePlayers) * homeFatigue;
                double homeDefense = (CalculateDefensePower(activeHome, homePlayers) * homeFatigue) + 5; // +5 Домакинско предимство

                double awayAttack = CalculateAttackPower(activeAway, awayPlayers) * awayFatigue;
                double awayDefense = CalculateDefensePower(activeAway, awayPlayers) * awayFatigue;

                double homeGoalChance = 0.015 + ((homeAttack - awayDefense) / 4000.0);
                double awayGoalChance = 0.012 + ((awayAttack - homeDefense) / 4000.0);

                if (_rand.NextDouble() < Math.Max(0.001, homeGoalChance))
                {
                    match.HomeGoals++;
                    AssignGoalEvent(activeHome, homePlayers);
                }
                else if (_rand.NextDouble() < Math.Max(0.001, awayGoalChance))
                {
                    match.AwayGoals++;
                    AssignGoalEvent(activeAway, awayPlayers);
                }

                ProcessDisciplineAndInjuries(activeHome, homePlayers, minute);
                ProcessDisciplineAndInjuries(activeAway, awayPlayers, minute);
            }

            match.IsPlayed = true;
            CalculateFinalRatings(match, homePlayers, match.HomeGoals, match.AwayGoals);
            CalculateFinalRatings(match, awayPlayers, match.AwayGoals, match.HomeGoals);

            return Task.FromResult(match);
        }

        private void InitializePerformances(Fixture match, List<Player> teamPlayers)
        {
            int matchSeasonId = match.SeasonId;

            foreach (var p in teamPlayers)
            {
                match.Performances.Add(new PlayerMatchPerformance
                {
                    PlayerId = p.Id,
                    FixtureId = match.Id,
                    SeasonId = matchSeasonId,
                    MinutesPlayed = 90,
                    MatchRating = 6.0m,
                    Goals = 0,
                    Assists = 0,
                    YellowCards = 0,
                    RedCards = 0,
                    InjuryDays = 0,
                    Player = p
                });
            }
        }

        // ==========================================
        // 🧠 НОВАТА УМНА МАТЕМАТИКА 
        // ==========================================

        private double CalculateFatigue(List<PlayerMatchPerformance> activePerformances, List<Player> roster, int minute)
        {
            if (minute <= 60) return 1.0; // Няма умора до 60-тата минута

            var activePlayers = roster.Where(p => activePerformances.Any(ap => ap.PlayerId == p.Id)).ToList();
            if (!activePlayers.Any()) return 1.0;

            // Средна стамина на отбора (0-100)
            double avgStamina = activePlayers.Average(p => p.Attributes.Stamina);

            // Колкото е по-малка стамината, толкова повече пада коефициентът в края на мача (от 1.0 до ~0.7)
            double staminaFactor = avgStamina / 100.0;
            double timePenalty = (minute - 60) / 30.0; // от 0.0 до 1.0 (в 90-та минута)

            double fatigueMultiplier = 1.0 - (timePenalty * (1.0 - staminaFactor) * 0.5);
            return Math.Clamp(fatigueMultiplier, 0.5, 1.0);
        }

        private double CalculateAttackPower(List<PlayerMatchPerformance> activePerformances, List<Player> roster)
        {
            if (!activePerformances.Any()) return 10;
            var activePlayers = roster.Where(p => activePerformances.Any(ap => ap.PlayerId == p.Id)).ToList();

            double totalAttack = 0;
            foreach (var p in activePlayers)
            {
                // Атаката се базира на уменията, умножени по тяхната позиционна важност (Weight)
                totalAttack += (double)(
                    (p.Attributes.Shooting * p.Position.ShootingWeight * 2.5m) +
                    (p.Attributes.Vision * p.Position.VisionWeight * 1.5m) +
                    (p.Attributes.Passing * p.Position.PassingWeight * 1.0m) +
                    (p.Attributes.Dribbling * p.Position.DribblingWeight * 1.0m) +
                    (p.Attributes.Pace * p.Position.PaceWeight * 0.5m)
                );
            }

            return totalAttack / activePlayers.Count;
        }

        private double CalculateDefensePower(List<PlayerMatchPerformance> activePerformances, List<Player> roster)
        {
            if (!activePerformances.Any()) return 10;
            var activePlayers = roster.Where(p => activePerformances.Any(ap => ap.PlayerId == p.Id)).ToList();

            double totalDefense = 0;
            foreach (var p in activePlayers)
            {
                // Защитата включва и вратаря, който изпъква благодарение на огромния си GoalkeepingWeight
                totalDefense += (double)(
                    (p.Attributes.Defending * p.Position.DefendingWeight * 2.0m) +
                    (p.Attributes.Goalkeeping * p.Position.GoalkeepingWeight * 4.0m) +
                    (p.Attributes.Physical * p.Position.PhysicalWeight * 1.0m) +
                    (p.Attributes.Stamina * p.Position.StaminaWeight * 0.5m)
                );
            }

            return totalDefense / activePlayers.Count;
        }

        private void AssignGoalEvent(List<PlayerMatchPerformance> activePerformances, List<Player> roster)
        {
            if (!activePerformances.Any()) return;

            // Кой ще вкара гола?
            var scorerProb = activePerformances.Select(perf => {
                var p = roster.First(r => r.Id == perf.PlayerId);
                // Шансът за гол е правопропорционален на Shooting, умножен по тежестта на позицията му.
                // Добавяме +1 базов шанс, за да може защитник (ShootingWeight 0) да вкара от корнер с глава!
                int weight = 1 + (int)(p.Attributes.Shooting * p.Position.ShootingWeight * 10);
                return new { Perf = perf, Weight = weight };
            }).ToList();

            var scorerData = GetWeightedRandom(scorerProb, x => x.Weight);
            scorerData.Perf.Goals++;

            // 80% шанс голът да има асистенция
            if (_rand.NextDouble() < 0.80)
            {
                var assistProb = activePerformances.Where(p => p.PlayerId != scorerData.Perf.PlayerId).Select(perf => {
                    var p = roster.First(r => r.Id == perf.PlayerId);
                    // Шансът за пас зависи от Vision и Passing, умножени по позиционната им тежест.
                    int weight = 1 + (int)((p.Attributes.Passing * p.Position.PassingWeight + p.Attributes.Vision * p.Position.VisionWeight) * 10);
                    return new { Perf = perf, Weight = weight };
                }).ToList();

                if (assistProb.Any())
                {
                    var assisterData = GetWeightedRandom(assistProb, x => x.Weight);
                    assisterData.Perf.Assists++;
                }
            }
        }

        private void ProcessDisciplineAndInjuries(List<PlayerMatchPerformance> activePerformances, List<Player> roster, int currentMinute)
        {
            foreach (var perf in activePerformances.ToList())
            {
                var player = roster.First(r => r.Id == perf.PlayerId);

                // Картони
                if (_rand.NextDouble() < 0.0005)
                {
                    if (perf.YellowCards == 0)
                    {
                        perf.YellowCards++;
                        if (_rand.NextDouble() < 0.05)
                        {
                            perf.RedCards++;
                            perf.MinutesPlayed = currentMinute;
                        }
                    }
                    else
                    {
                        perf.YellowCards++;
                        perf.RedCards++;
                        perf.MinutesPlayed = currentMinute;
                    }
                }

                // Контузии: Вече зависи и от Stamina! Играч с ниска издръжливост се контузва по-лесно към края.
                double fatigueRisk = currentMinute > 60 ? (100 - player.Attributes.Stamina) * 0.00001 : 0;
                double injuryChance = (player.Attributes.InjuryProne * 0.00003) + fatigueRisk;

                if (perf.RedCards == 0 && _rand.NextDouble() < injuryChance)
                {
                    perf.InjuryDays = _rand.Next(3, 30);
                    perf.MinutesPlayed = currentMinute;
                }
            }
        }

        private void CalculateFinalRatings(Fixture match, List<Player> teamPlayers, int teamGoals, int opponentGoals)
        {
            bool isWin = teamGoals > opponentGoals;
            bool isCleanSheet = opponentGoals == 0;

            foreach (var p in teamPlayers)
            {
                var perf = match.Performances.First(pf => pf.PlayerId == p.Id);
                decimal rating = 6.0m;

                if (isWin) rating += 0.5m;
                else if (teamGoals < opponentGoals) rating -= 0.3m;

                rating += (perf.Goals * 1.2m);
                rating += (perf.Assists * 0.8m);

                if (perf.YellowCards > 0) rating -= 0.5m;
                if (perf.RedCards > 0) rating -= 1.5m;

                // Оценка за Защитници и Вратари (ползваме позиционната тежест за по-универсална проверка)
                if (p.Position.DefendingWeight > 0.3m || p.Position.GoalkeepingWeight > 0.5m)
                {
                    if (isCleanSheet && perf.MinutesPlayed > 60) rating += 1.0m;
                    else rating -= (opponentGoals * 0.3m);
                }

                perf.MatchRating = Math.Clamp(Math.Round(rating, 1), 3.0m, 10.0m);
            }
        }

        private T GetWeightedRandom<T>(List<T> items, Func<T, int> weightSelector)
        {
            int totalWeight = items.Sum(weightSelector);
            if (totalWeight <= 0) return items.First();

            int randomValue = _rand.Next(0, totalWeight);

            foreach (var item in items)
            {
                randomValue -= weightSelector(item);
                if (randomValue <= 0) return item;
            }
            return items.Last();
        }
    }
}