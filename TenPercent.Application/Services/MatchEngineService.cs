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

                double homeAttack = CalculateAttackPower(activeHome, homePlayers);
                double homeDefense = CalculateDefensePower(activeHome, homePlayers) + 5;

                double awayAttack = CalculateAttackPower(activeAway, awayPlayers);
                double awayDefense = CalculateDefensePower(activeAway, awayPlayers);

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
            foreach (var p in teamPlayers)
            {
                match.Performances.Add(new PlayerMatchPerformance
                {
                    PlayerId = p.Id,
                    FixtureId = match.Id,
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
        private double CalculateAttackPower(List<PlayerMatchPerformance> activePerformances, List<Player> roster)
        {
            if (!activePerformances.Any()) return 10;
            var activePlayers = roster.Where(p => activePerformances.Any(ap => ap.PlayerId == p.Id)).ToList();
            var attackers = activePlayers.Where(p => p.Position.Abbreviation == "ST" || p.Position.Abbreviation == "MID").ToList();
            if (!attackers.Any()) return 30;

            return attackers.Average(p => p.Attributes.Shooting * 0.5 + p.Attributes.Passing * 0.3 + p.Attributes.Pace * 0.2);
        }

        private double CalculateDefensePower(List<PlayerMatchPerformance> activePerformances, List<Player> roster)
        {
            if (!activePerformances.Any()) return 10;
            var activePlayers = roster.Where(p => activePerformances.Any(ap => ap.PlayerId == p.Id)).ToList();
            var defenders = activePlayers.Where(p => p.Position.Abbreviation == "DEF" || p.Position.Abbreviation == "GK" || p.Position.Abbreviation == "MID").ToList();
            if (!defenders.Any()) return 30;

            return defenders.Average(p => p.Attributes.Defending * 0.6 + p.Attributes.Physical * 0.4);
        }

        private void AssignGoalEvent(List<PlayerMatchPerformance> activePerformances, List<Player> roster)
        {
            if (!activePerformances.Any()) return;

            var scorerProb = activePerformances.Select(perf => {
                var p = roster.First(r => r.Id == perf.PlayerId);
                int weight = p.Position.Abbreviation == "ST" ? p.Attributes.Shooting * 3 : (p.Position.Abbreviation == "MID" ? p.Attributes.Shooting : 10);
                return new { Perf = perf, Weight = weight };
            }).ToList();

            // ПРОМЯНА ТУК: Вече ползваме Generic метода и подаваме селектор за тежестта
            var scorerData = GetWeightedRandom(scorerProb, x => x.Weight);
            scorerData.Perf.Goals++;

            if (_rand.NextDouble() < 0.80)
            {
                var assistProb = activePerformances.Where(p => p.PlayerId != scorerData.Perf.PlayerId).Select(perf => {
                    var p = roster.First(r => r.Id == perf.PlayerId);
                    int weight = p.Position.Abbreviation == "MID" ? p.Attributes.Passing * 3 : p.Attributes.Passing;
                    return new { Perf = perf, Weight = weight };
                }).ToList();

                if (assistProb.Any())
                {
                    // ПРОМЯНА ТУК
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

                if (perf.RedCards == 0 && _rand.NextDouble() < (player.Attributes.InjuryProne * 0.00005))
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

                if (p.Position.Abbreviation == "DEF" || p.Position.Abbreviation == "GK")
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
            if (totalWeight <= 0) return items.First(); // Fallback

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