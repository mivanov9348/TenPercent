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
        private readonly IMessageService _messageService;

        public NegotiationService(AppDbContext context, IFinanceService financeService, IMessageService messageService)
        {
            _context = context;
            _financeService = financeService;
            _messageService = messageService;
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

            // ==========================================
            // --- НОВО: ВЗИМАМЕ ТЕКУЩИЯ СЕЗОН ---
            // ==========================================
            var worldState = await _context.WorldStates.FirstOrDefaultAsync();
            int currentSeasonNumber = 1;
            if (worldState != null && worldState.CurrentSeasonId.HasValue)
            {
                var activeSeason = await _context.Seasons.FindAsync(worldState.CurrentSeasonId.Value);
                if (activeSeason != null) currentSeasonNumber = activeSeason.SeasonNumber;
            }

            int legalDeptLevel = 1;
            int prMediaLevel = 1;

            decimal greedFactor = player.Attributes.Greed / 100m;
            decimal ambitionFactor = player.Attributes.Ambition / 100m;
            decimal loyaltyFactor = player.Attributes.Loyalty / 100m;

            decimal expectedBonus = player.MarketValue * 0.003m * (1m + greedFactor);
            if (expectedBonus < 1000m) expectedBonus = 1000m;

            decimal expectedTotalCommission = 18m - (greedFactor * 10m) + (legalDeptLevel * 1m);

            int expectedDuration = 3;
            if (player.Age <= 23 && player.Attributes.Ambition > 70) expectedDuration = 2;
            else if (player.Age >= 30) expectedDuration = 4;

            double score = 50.0;

            decimal demandedBonus = Math.Round(expectedBonus / 1000m) * 1000m;
            decimal maxAcceptedWageComm = Math.Round(expectedTotalCommission * 0.6m, 1);
            decimal maxAcceptedTransComm = Math.Round(expectedTotalCommission * 0.4m, 1);
            decimal totalOfferedCommission = offer.WageCommissionPercentage + offer.TransferCommissionPercentage;

            if (offer.SigningBonusPaid >= demandedBonus &&
                offer.WageCommissionPercentage <= maxAcceptedWageComm &&
                offer.TransferCommissionPercentage <= maxAcceptedTransComm &&
                offer.DurationYears == expectedDuration)
            {
                score += 100;
            }
            else
            {
                double bonusRatio = (double)(offer.SigningBonusPaid / expectedBonus);
                score += (bonusRatio * 20.0);

                decimal commissionDiff = totalOfferedCommission - expectedTotalCommission;
                if (commissionDiff > 0)
                {
                    score -= (double)commissionDiff * (3.0 + (double)greedFactor * 4.0);
                }

                int durationDiff = Math.Abs(offer.DurationYears - expectedDuration);
                if (durationDiff > 0)
                {
                    score -= (durationDiff * 5.0);
                }

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
                    score += (double)(player.Attributes.Loyalty * 0.4m);
                }

                int rngSwing = player.Attributes.Loyalty > 70 ? 5 : 10;
                score += _rand.Next(-rngSwing, rngSwing + 1);
            }

            double rejectionThreshold = 30.0 - ((double)loyaltyFactor * 10.0);

            if (score >= 80)
            {
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

                    // --- НОВО: ОБНОВЕНИТЕ ПОЛЕТА НА ДОГОВОРА ---
                    var contract = new RepresentationContract
                    {
                        PlayerId = player.Id,
                        AgencyId = agency.Id,
                        StartSeasonNumber = currentSeasonNumber,
                        EndSeasonNumber = currentSeasonNumber + offer.DurationYears,
                        IncomeCommissionPercentage = offer.WageCommissionPercentage, // DTO-то все още използва WageCommissionPercentage
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
                string rejectReason = "Офертата ви е изключително слаба и обидна.";
                if (Math.Abs(offer.DurationYears - expectedDuration) >= 2) rejectReason = "Не се разбрахте за продължителността на договора.";
                else if (totalOfferedCommission - expectedTotalCommission > 5) rejectReason = "Опитвате се да му вземете твърде голяма комисионна.";

                return new ContractResponseDto { Status = "Rejected", Message = $"{player.Name} стана от масата и си тръгна. {rejectReason}" };
            }
        }
    
    
    
    public async Task<(bool Success, string Message)> SendPoachOfferAsync(int sendingUserId, AgencyPoachOfferDto offerDto)
        {
            var senderAgent = await _context.Agents.Include(a => a.Agency).FirstOrDefaultAsync(a => a.UserId == sendingUserId);
            if (senderAgent?.Agency == null) return (false, "Нямате агенция.");

            var player = await _context.Players.Include(p => p.Agency).FirstOrDefaultAsync(p => p.Id == offerDto.TargetPlayerId);

            if (player == null) return (false, "Играчът не съществува.");
            if (player.AgencyId == null) return (false, "Този играч няма агент. Използвайте Pitch Player.");
            if (player.AgencyId == senderAgent.Agency.Id) return (false, "Това вече е ваш клиент!");

            if (senderAgent.Agency.Budget < offerDto.OfferedAmount) return (false, "Нямате достатъчно бюджет за тази оферта.");

            var placeholders = new Dictionary<string, string>
            {
                { "SenderName", senderAgent.Agency.Name },
                { "ReceiverName", player.Agency!.Name },
                { "PlayerName", player.Name },
                { "OfferAmount", offerDto.OfferedAmount.ToString("N0") }
            };

            // Пращаме шаблона. 
            // Забележи: RelatedEntityId = player.Id, DataValue = Офертата!
            var message = await _messageService.SendTemplatedMessageAsync(
                receiverAgencyId: player.AgencyId,
                senderType: EntityType.Agency,
                senderId: senderAgent.Agency.Id,
                senderName: senderAgent.Agency.Name,
                type: MessageType.TransferOffer, // Тригерира Accept/Reject бутони
                placeholders: placeholders,
                relatedEntityId: player.Id
            );

            // Тъй като SendTemplatedMessageAsync не приема DataValue, трябва да го ъпдейтнем ръчно
            message.DataValue = offerDto.OfferedAmount;
            await _context.SaveChangesAsync();

            return (true, $"Оферта от ${offerDto.OfferedAmount:N0} беше изпратена до агенцията на играча ({player.Agency.Name}).");
        }

        // =========================================================================
        // ОТГОВОР НА ОФЕРТАТА (ACCEPT / REJECT)
        // =========================================================================
        public async Task<(bool Success, string Message)> RespondToPoachOfferAsync(int respondingUserId, RespondToMessageOfferDto responseDto)
        {
            var responderAgent = await _context.Agents.Include(a => a.Agency).FirstOrDefaultAsync(a => a.UserId == respondingUserId);
            if (responderAgent?.Agency == null) return (false, "Нямате агенция.");

            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == responseDto.MessageId && m.ReceiverAgencyId == responderAgent.Agency.Id);

            if (message == null) return (false, "Съобщението не е намерено.");
            if (message.Type != MessageType.TransferOffer) return (false, "Това съобщение не е оферта.");
            if (message.RelatedEntityId == null) return (false, "Грешка в данните на офертата.");
            if (message.IsActioned) return (false, "Вече сте отговорили на тази оферта.");

            var targetPlayer = await _context.Players.Include(p => p.RepresentationContracts).FirstOrDefaultAsync(p => p.Id == message.RelatedEntityId.Value);
            if (targetPlayer == null || targetPlayer.AgencyId != responderAgent.Agency.Id)
                return (false, "Играчът вече не е ваш клиент.");

            decimal offeredAmount = message.DataValue ?? 0;

            if (responseDto.IsAccepted)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Извършваме транзакцията (Агенция А плаща на Агенция Б)
                    // message.SenderId е ID-то на Агенцията, която купува!
                    if (offeredAmount > 0)
                    {
                        var financeResult = await _financeService.ProcessTransactionAsync(
                            EntityType.Agency, message.SenderId,
                            EntityType.Agency, responderAgent.Agency.Id,
                            offeredAmount, TransactionCategory.TransferFee, // Може да се наложи да добавиш TransferFee в Enum-а
                            $"Buyout fee for {targetPlayer.Name} rights"
                        );

                        if (!financeResult.Success) throw new Exception(financeResult.Message);
                    }

                    // 2. Деактивираме стария договор
                    var oldContract = targetPlayer.RepresentationContracts.FirstOrDefault(c => c.IsActive);
                    if (oldContract != null) oldContract.IsActive = false;

                    // 3. Сменяме агента
                    targetPlayer.AgencyId = message.SenderId;

                    // В реална ситуация тук трябва да се генерира нов RepresentationContract за новия агент (по договаряне).
                    // Засега просто местим играча.

                    // 4. Известяваме купувача
                    await _messageService.SendMessageAsync(
                        message.SenderId, EntityType.Agency, responderAgent.Agency.Id, responderAgent.Agency.Name,
                        "Офертата е приета!", $"Приехме вашата оферта за {targetPlayer.Name}. Той вече е ваш клиент.", MessageType.Info);

                    message.IsActioned = true;
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return (true, "Вие приехте офертата и прехвърлихте правата.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return (false, "Грешка при трансфера: " + ex.Message);
                }
            }
            else
            {
                // Отказ
                await _messageService.SendMessageAsync(
                    message.SenderId, EntityType.Agency, responderAgent.Agency.Id, responderAgent.Agency.Name,
                    "Офертата е отхвърлена", $"Отхвърлихме вашата оферта за {targetPlayer.Name}.", MessageType.Info);

                message.IsActioned = true;
                await _context.SaveChangesAsync();
                return (true, "Вие отхвърлихте офертата.");
            }
        }
    }
}