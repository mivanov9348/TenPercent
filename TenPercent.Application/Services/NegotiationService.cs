namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Threading.Tasks;
    using TenPercent.Application.DTOs;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.Models;
    using TenPercent.Data.Enums;

    public class NegotiationService : INegotiationService
    {
        private readonly AppDbContext _context;
        private readonly IFinanceService _financeService;
        private readonly Random _rand = new Random();

        public NegotiationService(AppDbContext context, IFinanceService financeService)
        {
            _context = context;
            _financeService = financeService;
        }

        public async Task<ContractResponseDto> ProposeContractAsync(int userId, ContractOfferDto offer)
        {
            return await EvaluateOfferAsync(userId, offer, isRenewal: false);
        }

        public async Task<ContractResponseDto> RenewContractAsync(int userId, ContractOfferDto offer)
        {
            return await EvaluateOfferAsync(userId, offer, isRenewal: true);
        }

        private async Task<ContractResponseDto> EvaluateOfferAsync(int userId, ContractOfferDto offer, bool isRenewal)
        {
            var agent = await _context.Agents.Include(a => a.Agency).FirstOrDefaultAsync(a => a.UserId == userId);
            if (agent?.Agency == null) return new ContractResponseDto { Status = "Error", Message = "Агенцията не е намерена." };

            var agency = agent.Agency;
            var player = await _context.Players.Include(p => p.Attributes).FirstOrDefaultAsync(p => p.Id == offer.PlayerId);

            if (player == null) return new ContractResponseDto { Status = "Error", Message = "Играчът не съществува." };
            if (agency.Budget < offer.SigningBonusPaid) return new ContractResponseDto { Status = "Error", Message = "Нямате достатъчно бюджет." };

            if (!isRenewal && player.AgencyId.HasValue) return new ContractResponseDto { Status = "Error", Message = "Този играч вече има агент." };
            if (isRenewal && player.AgencyId != agency.Id) return new ContractResponseDto { Status = "Error", Message = "Този играч не е ваш клиент." };

            // ====================================================
            // 🏭 БЪДЕЩИ СГРАДИ / UPGRADES (Hooks)
            // ====================================================
            // В бъдеще тук ще четеш: int legalLevel = agency.Facilities.LegalDepartmentLevel;
            int legalDeptLevel = 1; // От 1 до 5 (Намалява недоволството от високи комисионни)
            int prMediaLevel = 1;   // От 1 до 5 (Вдига ефективната ти репутация в очите на играча)

            // ====================================================
            // 🧠 AI ЛОГИКА ЗА ХАРАКТЕРА
            // ====================================================
            decimal greedFactor = player.Attributes.Greed / 100m;
            decimal ambitionFactor = player.Attributes.Ambition / 100m;
            decimal loyaltyFactor = player.Attributes.Loyalty / 100m;

            // 1. ОЧАКВАНИ ПАРИ (Намалени!) 
            // 0.3% от маркет стойността (Играч за $10M ще иска базов бонус $30,000)
            decimal expectedBonus = player.MarketValue * 0.003m * (1m + greedFactor);
            if (expectedBonus < 1000m) expectedBonus = 1000m;

            // 2. ОЧАКВАНИ КОМИСИОННИ (Влияят се от Legal Department)
            // Legal Department Level помага играчът да приеме по-голям процент за агенцията
            decimal expectedTotalCommission = 18m - (greedFactor * 10m) + (legalDeptLevel * 1m);

            // 3. ОЧАКВАНА ПРОДЪЛЖИТЕЛНОСТ (СЕЗОНИ)
            int expectedDuration = 3;
            if (player.Age <= 23 && player.Attributes.Ambition > 70) expectedDuration = 2; // Млади звезди искат кратки договори
            else if (player.Age >= 30) expectedDuration = 4; // Ветераните искат сигурност

            // --- СТАРТИРАМЕ ОЦЕНКАТА ---
            double score = 50.0;

            // Твърди граници на контра-офертата
            decimal demandedBonus = Math.Round(expectedBonus / 1000m) * 1000m;
            decimal maxAcceptedWageComm = Math.Round(expectedTotalCommission * 0.6m, 1);
            decimal maxAcceptedTransComm = Math.Round(expectedTotalCommission * 0.4m, 1);
            decimal totalOfferedCommission = offer.WageCommissionPercentage + offer.TransferCommissionPercentage;

            // АВТОМАТИЧНО ПРИЕМАНЕ (Ако си изпълнил точните условия от предишна контра-оферта)
            if (offer.SigningBonusPaid >= demandedBonus &&
                offer.WageCommissionPercentage <= maxAcceptedWageComm &&
                offer.TransferCommissionPercentage <= maxAcceptedTransComm &&
                offer.DurationYears == expectedDuration)
            {
                score += 100;
            }
            else
            {
                // A) Бонус
                double bonusRatio = (double)(offer.SigningBonusPaid / expectedBonus);
                score += (bonusRatio * 20.0);

                // Б) Комисионни (ЕКСТРЕМНО НАКАЗАНИЕ, АКО ГО ЛЪЖЕШ)
                decimal commissionDiff = totalOfferedCommission - expectedTotalCommission;
                if (commissionDiff > 0)
                {
                    // Наказанието е много по-строго! Умножаваме разликата по 4. 
                    // Ако искаш 5% отгоре, губиш 20 точки (или повече, ако е алчен)!
                    score -= (double)commissionDiff * (3.0 + (double)greedFactor * 4.0);
                }

                // В) Сезони (Наказание за грешна дължина на договора)
                int durationDiff = Math.Abs(offer.DurationYears - expectedDuration);
                if (durationDiff > 0)
                {
                    score -= (durationDiff * 5.0); // -5 точки за всеки сезон разминаване
                }

                // Г) Репутация (Подпомогната от PR & Media)
                if (!isRenewal)
                {
                    double effectiveReputation = agency.Reputation + (prMediaLevel * 3.0);
                    double expectedReputation = (double)player.Attributes.Ambition;
                    double reputationDiff = effectiveReputation - expectedReputation;
                    if (reputationDiff < 0) score += reputationDiff * (0.5 + (double)ambitionFactor);
                    else score += reputationDiff * 0.2;
                }
                else
                {
                    score += (double)(player.Attributes.Loyalty * 0.4m); // Оценява лоялността
                }

                // Д) Настроение (RNG)
                int rngSwing = player.Attributes.Loyalty > 70 ? 5 : 10;
                score += _rand.Next(-rngSwing, rngSwing + 1);
            }

            double rejectionThreshold = 30.0 - ((double)loyaltyFactor * 10.0);

            // ====================================================
            // 📝 РЕШЕНИЕТО НА ИГРАЧА
            // ====================================================

            if (score >= 80)
            {
                // --- ПРИЕМА ---
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    if (offer.SigningBonusPaid > 0)
                    {
                        var financeResult = await _financeService.ProcessTransactionAsync(
                            EntityType.Agency, agency.Id, EntityType.Player, player.Id,
                            offer.SigningBonusPaid, TransactionCategory.SigningBonus,
                            isRenewal ? $"Renewal bonus paid to {player.Name}" : $"Signing bonus paid to {player.Name}");

                        if (!financeResult.Success) { await transaction.RollbackAsync(); return new ContractResponseDto { Status = "Error", Message = financeResult.Message }; }
                    }

                    if (isRenewal)
                    {
                        var oldContract = await _context.RepresentationContracts.FirstOrDefaultAsync(c => c.PlayerId == player.Id && c.IsActive);
                        if (oldContract != null) oldContract.IsActive = false;
                    }

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

                    return new ContractResponseDto { Status = "Accepted", Message = isRenewal ? $"{player.Name} преподписа договора си!" : $"{player.Name} подписа с агенцията ви!" };
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return new ContractResponseDto { Status = "Error", Message = "Грешка в базата: " + ex.Message };
                }
            }
            else if (score >= rejectionThreshold && score < 80)
            {
                // --- КОНТРА ОФЕРТА ---
                // Тук включваме изискването за сезоните!
                string durationDemand = expectedDuration != offer.DurationYears ? $"Държи договорът да е за точно {expectedDuration} сезона." : "Продължителността го устройва.";

                return new ContractResponseDto
                {
                    Status = "CounterOffer",
                    Message = $"{player.Name} не е съгласен, но дава контра-оферта. {durationDemand}",
                    CounterSigningBonus = demandedBonus,
                    CounterWageCommission = offer.WageCommissionPercentage > maxAcceptedWageComm ? maxAcceptedWageComm : offer.WageCommissionPercentage,
                    CounterTransferCommission = offer.TransferCommissionPercentage > maxAcceptedTransComm ? maxAcceptedTransComm : offer.TransferCommissionPercentage
                };
            }
            else
            {
                // --- ОТКАЗВА ---
                string rejectReason = "Офертата ви е изключително слаба и обидна.";
                if (Math.Abs(offer.DurationYears - expectedDuration) >= 2) rejectReason = "Не се разбрахте за продължителността на договора.";
                else if (totalOfferedCommission - expectedTotalCommission > 5) rejectReason = "Опитвате се да му вземете твърде голяма комисионна.";

                return new ContractResponseDto { Status = "Rejected", Message = $"{player.Name} стана от масата и си тръгна. {rejectReason}" };
            }
        }
    }
}