namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.Models;
    public class ScoutReportGenerator : IScoutReportGenerator
    {
        private readonly AppDbContext _context;
        private readonly Random _rand = new Random();

        public ScoutReportGenerator(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ScoutReport> GenerateReportAsync(ScoutReport report, Player player, int knowledgeLevel)
        {
            var attr = player.Attributes;
            string pos = player.Position.Abbreviation;

            // Дърпаме всички шаблони от базата, за да ги филтрираме бързо в паметта
            var allTemplates = await _context.ScoutTemplates.ToListAsync();

            var strengths = new List<string>();
            var weaknesses = new List<string>();

            // --- ПОМОЩНА ФУНКЦИЯ ЗА ТЪРСЕНЕ НА ФРАЗИ ---
            void EvaluateAttribute(string category, string attrName, int attrValue, List<string> listToFill)
            {
                var matchingTemplates = allTemplates
                    .Where(t => t.Category == category
                             && t.AttributeName == attrName
                             && attrValue >= t.MinValue
                             && attrValue <= t.MaxValue
                             && (string.IsNullOrEmpty(t.TargetPosition) || t.TargetPosition == pos))
                    .ToList();

                if (matchingTemplates.Any())
                {
                    // Избираме рандъм фраза от намерените
                    var selectedPhrase = matchingTemplates[_rand.Next(matchingTemplates.Count)];
                    listToFill.Add(selectedPhrase.Text);
                }
            }

            // Оценяваме Силни страни (Търсим високи атрибути)
            EvaluateAttribute("Strength", "Pace", attr.Pace, strengths);
            EvaluateAttribute("Strength", "Shooting", attr.Shooting, strengths);
            EvaluateAttribute("Strength", "Passing", attr.Passing, strengths);
            EvaluateAttribute("Strength", "Dribbling", attr.Dribbling, strengths);
            EvaluateAttribute("Strength", "Defending", attr.Defending, strengths);
            EvaluateAttribute("Strength", "Physical", attr.Physical, strengths);

            // Оценяваме Слаби страни (Търсим ниски атрибути или висока склонност към контузии)
            EvaluateAttribute("Weakness", "Stamina", attr.Stamina, weaknesses);
            EvaluateAttribute("Weakness", "InjuryProne", attr.InjuryProne, weaknesses);
            EvaluateAttribute("Weakness", "Pace", attr.Pace, weaknesses);
            EvaluateAttribute("Weakness", "Defending", attr.Defending, weaknesses);

            // Резервни фрази, ако не намерим нищо
            if (strengths.Count == 0) strengths.Add("Добре балансиран играч, но без изявени качества.");
            if (weaknesses.Count == 0) weaknesses.Add("Няма очевидни слаби звена в играта си.");

            // Разбъркваме списъците и взимаме топ 2 (за да не е прекалено дълъг доклада)
            report.Strengths = string.Join(" ", strengths.OrderBy(x => _rand.Next()).Take(2));
            report.Weaknesses = string.Join(" ", weaknesses.OrderBy(x => _rand.Next()).Take(2));

            // --- ХАРАКТЕР (Personality) ---
            if (knowledgeLevel >= 3)
            {
                var personalityPhrases = new List<string>();
                EvaluateAttribute("Personality", "Greed", attr.Greed, personalityPhrases);
                EvaluateAttribute("Personality", "Loyalty", attr.Loyalty, personalityPhrases);
                EvaluateAttribute("Personality", "Ambition", attr.Ambition, personalityPhrases);

                report.PersonalityNotes = personalityPhrases.Any()
                    ? personalityPhrases.First() // Взимаме само 1 изявена черта
                    : "Нормален професионалист. Не създава проблеми.";
            }
            else
            {
                report.PersonalityNotes = "Нужно е по-задълбочено проучване за характера на играча.";
            }

            // --- ПРЕПОРЪКА ОТ СКАУТА (Grade) ---
            if (knowledgeLevel < 2)
            {
                report.RecommendationGrade = "Monitor (Недостатъчно данни)";
            }
            else
            {
                int potentialDiff = player.PotentialAbility - player.CurrentAbility;
                if (player.CurrentAbility >= 85 || potentialDiff >= 15) report.RecommendationGrade = "A+ (Sign Immediately)";
                else if (player.CurrentAbility >= 75 || potentialDiff >= 10) report.RecommendationGrade = "B (Solid Addition)";
                else if (player.CurrentAbility >= 65) report.RecommendationGrade = "C (Squad Player)";
                else report.RecommendationGrade = "D (Avoid)";
            }

            // --- ОЧАКВАНА ЦЕНА (С лека грешка спрямо Knowledge Level) ---
            double errorMargin = (5 - knowledgeLevel) * 0.1;
            decimal errorMultiplier = 1m + (decimal)(_rand.NextDouble() * errorMargin * 2 - errorMargin);

            report.EstimatedMarketValue = Math.Round(player.MarketValue * errorMultiplier, 0);
            report.EstimatedWageDemand = Math.Round((player.CurrentAbility * 1000m) * errorMultiplier, 0);

            // --- OVR и POT (Маскирани диапазони) ---
            int range = (5 - knowledgeLevel) * 3;
            report.MinEstimatedOVR = Math.Max(1, player.CurrentAbility - range);
            report.MaxEstimatedOVR = Math.Min(99, player.CurrentAbility + range);
            report.MinEstimatedPOT = Math.Max(1, player.PotentialAbility - range);
            report.MaxEstimatedPOT = Math.Min(99, player.PotentialAbility + range);

            return report;
        }
    }
}