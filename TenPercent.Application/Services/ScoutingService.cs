namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading.Tasks;
    using TenPercent.Application.DTOs;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.Enums;
    using TenPercent.Data.Models;

    public class ScoutingService : IScoutingService
    {
        private readonly AppDbContext _context;
        private readonly IFinanceService _financeService;
        private readonly IScoutReportGenerator _reportGenerator;

        public ScoutingService(AppDbContext context, IFinanceService financeService, IScoutReportGenerator reportGenerator)
        {
            _context = context;
            _financeService = financeService;
            _reportGenerator = reportGenerator;
        }

        public async Task<(bool Success, string Message, object Report)> GeneratePaidReportAsync(int userId, int playerId)
        {
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent?.Agency == null) return (false, "Нямате регистрирана агенция.", null!);

            var agency = agent.Agency;

            var player = await _context.Players
                .Include(p => p.Attributes)
                .Include(p => p.Position)
                .FirstOrDefaultAsync(p => p.Id == playerId);

            if (player == null) return (false, "Играчът не е намерен.", null!);

            // 1. Проверяваме дали играчът е наш клиент
            bool isOwnClient = player.AgencyId == agency.Id;
            decimal reportCost = isOwnClient ? 0m : 10000m;

            // Ниво на детайлност: Засега фиксираме на 4 за чужди, 5 за наши (докато направим сградата)
            int knowledgeLevel = isOwnClient ? 5 : Math.Clamp(agency.Level + 2, 1, 5);

            // 2. Взимаме парите (ако не е наш клиент)
            if (reportCost > 0)
            {
                if (agency.Budget < reportCost)
                {
                    return (false, $"Нямате достатъчно бюджет за скаутване. Нужни са ви {reportCost:C0}.", null!);
                }

                var financeResult = await _financeService.ProcessTransactionAsync(
                    EntityType.Agency, agency.Id,
                    EntityType.Bank, 1,
                    reportCost,
                    TransactionCategory.Scouting, // Увери се, че си добавил Scouting в TransactionCategory Енума!
                    $"Scouting Report requested for {player.Name}"
                );

                if (!financeResult.Success)
                {
                    return (false, "Грешка при плащането на доклада: " + financeResult.Message, null!);
                }
            }

            // 3. Проверяваме дали вече имаме стар доклад
            var existingReport = await _context.ScoutReports
                .FirstOrDefaultAsync(r => r.AgencyId == agency.Id && r.PlayerId == player.Id);

            ScoutReport reportEntity;

            if (existingReport != null)
            {
                reportEntity = existingReport;
                reportEntity.GeneratedAt = DateTime.UtcNow;
                reportEntity.KnowledgeLevel = knowledgeLevel;
            }
            else
            {
                reportEntity = new ScoutReport
                {
                    AgencyId = agency.Id,
                    PlayerId = player.Id,
                    GeneratedAt = DateTime.UtcNow,
                    KnowledgeLevel = knowledgeLevel
                };
                _context.ScoutReports.Add(reportEntity);
            }

            // 4. Генерираме текста!
            reportEntity = await _reportGenerator.GenerateReportAsync(reportEntity, player, knowledgeLevel);

            await _context.SaveChangesAsync();

            // 5. Връщаме DTO
            var dto = MapToDto(reportEntity, player.Name);

            return (true, "Успешно генериран доклад!", dto);
        }

        public async Task<object?> GetReportAsync(int userId, int playerId)
        {
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent?.Agency == null) return null;

            var report = await _context.ScoutReports
                .Include(r => r.Player)
                .FirstOrDefaultAsync(r => r.AgencyId == agent.Agency.Id && r.PlayerId == playerId);

            if (report == null) return null;

            return MapToDto(report, report.Player.Name);
        }

        private ScoutReportDto MapToDto(ScoutReport report, string playerName)
        {
            return new ScoutReportDto
            {
                Id = report.Id,
                PlayerId = report.PlayerId,
                PlayerName = playerName,
                GeneratedAt = report.GeneratedAt,
                KnowledgeLevel = report.KnowledgeLevel,
                MinOVR = report.MinEstimatedOVR,
                MaxOVR = report.MaxEstimatedOVR,
                MinPOT = report.MinEstimatedPOT,
                MaxPOT = report.MaxEstimatedPOT,
                RecommendationGrade = report.RecommendationGrade,
                Strengths = report.Strengths,
                Weaknesses = report.Weaknesses,
                PersonalityNotes = report.PersonalityNotes,
                EstimatedValue = report.EstimatedMarketValue,
                EstimatedWage = report.EstimatedWageDemand
            };
        }
    }
}   