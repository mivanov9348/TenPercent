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
    using TenPercent.Data.Enums.TenPercent.Data.Models.Enums; // Увери се, че пътят към енума е верен!

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

            var allTemplates = await _context.ScoutTemplates.ToListAsync();

            var strengths = new List<string>();
            var weaknesses = new List<string>();

            // --- ПОМОЩНА ФУНКЦИЯ (ВЕЧЕ ПРИЕМА ENUM!) ---
            void EvaluateAttribute(ScoutCategory category, string attrName, int attrValue, List<string> listToFill)
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
                    var selectedPhrase = matchingTemplates[_rand.Next(matchingTemplates.Count)];
                    listToFill.Add(selectedPhrase.Text);
                }
            }

            // --- 1. СИЛНИ СТРАНИ ---
            EvaluateAttribute(ScoutCategory.Strength, "Pace", attr.Pace, strengths);
            EvaluateAttribute(ScoutCategory.Strength, "Shooting", attr.Shooting, strengths);
            EvaluateAttribute(ScoutCategory.Strength, "Passing", attr.Passing, strengths);
            EvaluateAttribute(ScoutCategory.Strength, "Dribbling", attr.Dribbling, strengths);
            EvaluateAttribute(ScoutCategory.Strength, "Defending", attr.Defending, strengths);
            EvaluateAttribute(ScoutCategory.Strength, "Physical", attr.Physical, strengths);
            EvaluateAttribute(ScoutCategory.Strength, "Goalkeeping", attr.Goalkeeping, strengths); // Важно за вратарите!

            // --- 2. СЛАБИ СТРАНИ ---
            EvaluateAttribute(ScoutCategory.Weakness, "Stamina", attr.Stamina, weaknesses);
            EvaluateAttribute(ScoutCategory.Weakness, "InjuryProne", attr.InjuryProne, weaknesses);
            EvaluateAttribute(ScoutCategory.Weakness, "Pace", attr.Pace, weaknesses);
            EvaluateAttribute(ScoutCategory.Weakness, "Defending", attr.Defending, weaknesses);
            EvaluateAttribute(ScoutCategory.Weakness, "Shooting", attr.Shooting, weaknesses);
            EvaluateAttribute(ScoutCategory.Weakness, "Passing", attr.Passing, weaknesses);

            // Резервни фрази
            if (strengths.Count == 0) strengths.Add("Добре балансиран играч, но без изявени качества.");
            if (weaknesses.Count == 0) weaknesses.Add("Няма очевидни слаби звена в играта си.");

            report.Strengths = string.Join(" ", strengths.OrderBy(x => _rand.Next()).Take(2));
            report.Weaknesses = string.Join(" ", weaknesses.OrderBy(x => _rand.Next()).Take(2));

            // --- 3. ХАРАКТЕР (Personality) ---
            if (knowledgeLevel >= 3)
            {
                var personalityPhrases = new List<string>();
                EvaluateAttribute(ScoutCategory.Personality, "Greed", attr.Greed, personalityPhrases);
                EvaluateAttribute(ScoutCategory.Personality, "Loyalty", attr.Loyalty, personalityPhrases);
                EvaluateAttribute(ScoutCategory.Personality, "Ambition", attr.Ambition, personalityPhrases);

                report.PersonalityNotes = personalityPhrases.Any()
                    ? personalityPhrases.First()
                    : "Нормален професионалист. Не създава проблеми.";
            }
            else
            {
                report.PersonalityNotes = "Нужно е по-задълбочено проучване за характера на играча.";
            }

            // --- 4. ПРЕПОРЪКА ОТ СКАУТА ---
            if (knowledgeLevel < 2)
            {
                report.RecommendationGrade = "Monitor (Недостатъчно данни)";
            }
            else
            {
                // За Препоръките можеш да ползваш директно темплейтите от базата или тази твърда логика!
                // Засега я оставяме твърда, защото е по-сложна математика.
                int potentialDiff = player.PotentialAbility - player.CurrentAbility;
                if (player.CurrentAbility >= 85 || potentialDiff >= 15) report.RecommendationGrade = "A+ (Sign Immediately)";
                else if (player.CurrentAbility >= 75 || potentialDiff >= 10) report.RecommendationGrade = "B (Solid Addition)";
                else if (player.CurrentAbility >= 65) report.RecommendationGrade = "C (Squad Player)";
                else report.RecommendationGrade = "D (Avoid)";
            }

            // --- 5. ОЧАКВАНА ЦЕНА И ЗАПЛАТА ---
            double errorMargin = (5 - knowledgeLevel) * 0.1;
            decimal errorMultiplier = 1m + (decimal)(_rand.NextDouble() * errorMargin * 2 - errorMargin);

            report.EstimatedMarketValue = Math.Round(player.MarketValue * errorMultiplier, 0);
            report.EstimatedWageDemand = Math.Round((player.CurrentAbility * 1000m) * errorMultiplier, 0);

            // --- 6. МАСКИРАНЕ НА OVR И POT ---
            int range = (5 - knowledgeLevel) * 3;
            report.MinEstimatedOVR = Math.Max(1, player.CurrentAbility - range);
            report.MaxEstimatedOVR = Math.Min(99, player.CurrentAbility + range);
            report.MinEstimatedPOT = Math.Max(1, player.PotentialAbility - range);
            report.MaxEstimatedPOT = Math.Min(99, player.PotentialAbility + range);

            return report;
        }
    }
}