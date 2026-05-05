namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading.Tasks;
    using TenPercent.Application.DTOs;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.Models;
    public class NegotiationService : INegotiationService
    {
        private readonly AppDbContext _context;
        private readonly Random _rand = new Random();

        public NegotiationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ContractResponseDto> ProposeContractAsync(int userId, ContractOfferDto offer)
        {
            var agent = await _context.Agents
                .Include(a => a.Agency)
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (agent?.Agency == null)
                return new ContractResponseDto { Status = "Error", Message = "Агенцията не е намерена." };

            var agency = agent.Agency;

            // ВЗИМАМЕ ИГРАЧА С НЕГОВИТЕ АТРИБУТИ
            var player = await _context.Players
                .Include(p => p.Attributes)
                .FirstOrDefaultAsync(p => p.Id == offer.PlayerId);

            if (player == null)
                return new ContractResponseDto { Status = "Error", Message = "Играчът не съществува." };

            if (player.AgencyId.HasValue)
                return new ContractResponseDto { Status = "Error", Message = "Този играч вече има агент." };

            if (agency.Budget < offer.SigningBonusPaid)
                return new ContractResponseDto { Status = "Error", Message = "Нямате достатъчно бюджет за този бонус." };

            // ====================================================
            // 🧠 AI ЛОГИКА ЗА ОЦЕНКА НА ОФЕРТАТА (Характер на играча)
            // ====================================================

            // 1. Коефициенти от атрибутите (от 0.01 до 0.99)
            decimal greedFactor = player.Attributes.Greed / 100m;
            decimal ambitionFactor = player.Attributes.Ambition / 100m;
            decimal loyaltyFactor = player.Attributes.Loyalty / 100m;

            // 2. Изчисляване на ОЧАКВАНИЯ бонус
            // Нормално играчът иска 5% от стойността си. Алчният (Greed=99) ще иска двойно (10%)!
            decimal expectedBonus = player.MarketValue * 0.05m * (1m + greedFactor);
            if (expectedBonus < 1000m) expectedBonus = 1000m; // Минимум бонус за най-евтините

            // Очаквана обща комисионна (Заплата + Трансфер). Средно е 15-20% общо.
            // Алчните играчи искат да задържат повече пари за себе си.
            decimal expectedTotalCommission = 25m - (greedFactor * 10m); // Алчен иска макс 15%, скромен дава до 25%

            // 3. Точкова система (Score) - Започваме от 50
            double score = 50.0;

            // --- Влияние на Парите (Signing Bonus) ---
            double bonusRatio = (double)(offer.SigningBonusPaid / expectedBonus);
            // Ако му дадеш повече, отколкото иска, се радва пропорционално
            score += (bonusRatio * 20.0);

            // --- Влияние на Комисионната (Алчност) ---
            decimal totalOfferedCommission = offer.WageCommissionPercentage + offer.TransferCommissionPercentage;
            decimal commissionDiff = totalOfferedCommission - expectedTotalCommission;

            // Ако му искаш по-голяма комисионна отколкото му харесва, алчността го ядосва по-бързо
            if (commissionDiff > 0)
            {
                score -= (double)commissionDiff * (1.0 + (double)greedFactor * 2.0);
            }

            // --- Влияние на Репутацията (Амбиция) ---
            // Амбициозният играч очаква агент с репутация близка до неговата амбиция
            double expectedReputation = (double)player.Attributes.Ambition;
            double reputationDiff = agency.Reputation - expectedReputation;

            // Ако си с по-ниска репутация, амбициозният играч те наказва жестоко
            if (reputationDiff < 0)
            {
                score += reputationDiff * (0.5 + (double)ambitionFactor);
            }
            else
            {
                score += reputationDiff * 0.2; // Бонус, ако си по-известен отколкото той очаква
            }

            // --- Малко RNG за реализъм (Търпение/Настроение) ---
            // Лоялните играчи са с по-постоянно настроение, нелоялните са непредвидими
            int rngSwing = player.Attributes.Loyalty > 70 ? 5 : 15;
            score += _rand.Next(-rngSwing, rngSwing + 1);

            // ====================================================
            // 📝 РЕШЕНИЕТО НА ИГРАЧА
            // ====================================================

            // Лоялните (търпеливи) играчи са по-склонни да преговарят, вместо да отказват директно
            double rejectionThreshold = 30.0 - ((double)loyaltyFactor * 10.0); // От 20 до 30

            if (score >= 80)
            {
                // ПРИЕМА ВЕДНАГА
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    agency.Budget -= offer.SigningBonusPaid;

                    var contract = new RepresentationContract
                    {
                        PlayerId = player.Id,
                        AgencyId = agency.Id,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddYears(offer.DurationYears),
                        WageCommissionPercentage = offer.WageCommissionPercentage,
                        TransferCommissionPercentage = offer.TransferCommissionPercentage,
                        SigningBonusPaid = offer.SigningBonusPaid,
                        AgencyReleaseClause = offer.AgencyReleaseClause,
                        IsActive = true
                    };

                    _context.RepresentationContracts.Add(contract);
                    player.AgencyId = agency.Id;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ContractResponseDto
                    {
                        Status = "Accepted",
                        Message = $"{player.Name} беше впечатлен от офертата и подписа с агенцията ви!"
                    };
                }
                catch
                {
                    await transaction.RollbackAsync();
                    return new ContractResponseDto { Status = "Error", Message = "Грешка при записването на договора в базата." };
                }
            }
            else if (score >= rejectionThreshold && score < 80)
            {
                // КОНТРА-ОФЕРТА (Преговаря)
                // Иска по-висок бонус или по-ниски комисионни
                decimal demandedBonus = offer.SigningBonusPaid < expectedBonus ? expectedBonus : offer.SigningBonusPaid * 1.1m;

                decimal maxAcceptedWageComm = expectedTotalCommission * 0.6m; // Разпределя 60% за Заплата
                decimal maxAcceptedTransComm = expectedTotalCommission * 0.4m; // Разпределя 40% за Трансфер

                return new ContractResponseDto
                {
                    Status = "CounterOffer",
                    Message = $"{player.Name} не е напълно съгласен, но е склонен на преговори. Ето неговите условия:",
                    CounterSigningBonus = Math.Round(demandedBonus / 1000m) * 1000m, // Закръгля на хилядарки
                    CounterWageCommission = offer.WageCommissionPercentage > maxAcceptedWageComm ? Math.Round(maxAcceptedWageComm, 1) : offer.WageCommissionPercentage,
                    CounterTransferCommission = offer.TransferCommissionPercentage > maxAcceptedTransComm ? Math.Round(maxAcceptedTransComm, 1) : offer.TransferCommissionPercentage
                };
            }
            else
            {
                // ОТКАЗВА КАТЕГОРИЧНО
                string rejectReason = "не хареса условията";
                if (reputationDiff < -20 && ambitionFactor > 0.7m) rejectReason = "смята, че агенцията ви е твърде малка за неговите амбиции";
                else if (bonusRatio < 0.5 && greedFactor > 0.7m) rejectReason = "смята бонуса за обидно нисък";

                return new ContractResponseDto
                {
                    Status = "Rejected",
                    Message = $"{player.Name} прекрати преговорите. Причина: {rejectReason}."
                };
            }
        }
    }
}