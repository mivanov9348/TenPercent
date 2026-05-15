namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
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

            // ========================================================
            // ФИКС: ПРОВЕРКА ЗА ВИСЯЩ ТРАНСФЕР (СТЪПКА 2)
            // ========================================================
            Message pendingTransfer = null;
            if (!isRenewal && player.AgencyId.HasValue)
            {
                pendingTransfer = await _context.Messages.FirstOrDefaultAsync(m =>
                    m.ReceiverAgencyId == agency.Id &&
                    m.Type == MessageType.ContractNegotiation &&
                    m.RelatedEntityId == player.Id &&
                    m.SenderId == player.AgencyId.Value &&
                    !m.IsActioned);

                if (pendingTransfer == null)
                    return new ContractResponseDto { Status = "Error", Message = "Този играч вече има агент и нямате договорена трансферна сума с неговата агенция." };

                decimal transferFee = pendingTransfer.DataValue ?? 0;
                if (agency.Budget < (offer.SigningBonusPaid + transferFee))
                    return new ContractResponseDto { Status = "Error", Message = $"Нямате достатъчно бюджет за трансферната сума (${transferFee:N0}) и бонуса на играча." };
            }
            else if (agency.Budget < offer.SigningBonusPaid)
            {
                return new ContractResponseDto { Status = "Error", Message = "Нямате достатъчно бюджет." };
            }

            if (isRenewal && player.AgencyId != agency.Id) return new ContractResponseDto { Status = "Error", Message = "Този играч не е ваш клиент." };

            // Взимаме текущия сезон
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

            if (score >= 80) // ИГРАЧЪТ ПРИЕМА ЛИЧНИТЕ УСЛОВИЯ
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. АКО Е ТРАНСФЕР -> ПЛАЩАМЕ НА ДРУГАТА АГЕНЦИЯ И ЗАТВАРЯМЕ СЪОБЩЕНИЕТО
                    if (pendingTransfer != null)
                    {
                        decimal transferFee = pendingTransfer.DataValue ?? 0;
                        if (transferFee > 0)
                        {
                            var transferFinance = await _financeService.ProcessTransactionAsync(
                                EntityType.Agency, agency.Id, EntityType.Agency, player.AgencyId.Value,
                                transferFee, TransactionCategory.TransferFee, $"Transfer fee paid for {player.Name}");

                            if (!transferFinance.Success) throw new Exception(transferFinance.Message);
                        }

                        var oldContract = await _context.RepresentationContracts.FirstOrDefaultAsync(c => c.PlayerId == player.Id && c.IsActive);
                        if (oldContract != null) oldContract.IsActive = false;

                        pendingTransfer.IsActioned = true; // Приключваме трансфера
                    }

                    // 2. ПЛАЩАМЕ БОНУСА НА ИГРАЧА (АКО ИМА)
                    if (offer.SigningBonusPaid > 0)
                    {
                        var financeResult = await _financeService.ProcessTransactionAsync(
                            EntityType.Agency, agency.Id, EntityType.Player, player.Id,
                            offer.SigningBonusPaid, TransactionCategory.SigningBonus,
                            isRenewal ? $"Renewal bonus paid to {player.Name}" : $"Signing bonus paid to {player.Name}");

                        if (!financeResult.Success) throw new Exception(financeResult.Message);
                    }

                    if (isRenewal)
                    {
                        var oldContract = await _context.RepresentationContracts.FirstOrDefaultAsync(c => c.PlayerId == player.Id && c.IsActive);
                        if (oldContract != null) oldContract.IsActive = false;
                    }

                    // 3. СЪЗДАВАМЕ НОВИЯ ДОГОВОР
                    var contract = new RepresentationContract
                    {
                        PlayerId = player.Id,
                        AgencyId = agency.Id,
                        StartSeasonNumber = currentSeasonNumber,
                        EndSeasonNumber = currentSeasonNumber + offer.DurationYears,
                        IncomeCommissionPercentage = offer.WageCommissionPercentage,
                        TransferCommissionPercentage = offer.TransferCommissionPercentage,
                        SigningBonusPaid = offer.SigningBonusPaid,
                        AgencyReleaseClause = offer.AgencyReleaseClause,
                        IsActive = true
                    };

                    _context.RepresentationContracts.Add(contract);
                    player.AgencyId = agency.Id;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return new ContractResponseDto { Status = "Accepted", Message = isRenewal ? $"{player.Name} преподписа договора си!" : $"Трансферът е успешен! {player.Name} подписа с агенцията ви." };
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

                // НОВО: Сделката пропада!
                if (pendingTransfer != null)
                {
                    pendingTransfer.IsActioned = true; // Маркираме трансфера като приключен/неуспешен

                    // Ако искаш да върнеш парите на купувача, защото сделката е пропаднала, 
                    // трябва да добавиш логика за възстановяване на бюджета тук (ако парите са му били блокирани).
                    // Зависи как си го имплементирал в Стъпка 1.

                    await _context.SaveChangesAsync(); // Запазваме статуса на съобщението!

                    rejectReason += " Трансферът е окончателно анулиран.";
                }

                return new ContractResponseDto { Status = "Rejected", Message = $"{player.Name} стана от масата и си тръгна. {rejectReason}" };
            }
        }

        public async Task<(bool Success, string Message)> SendPoachOfferAsync(int sendingUserId, AgencyPoachOfferDto offerDto)
        {
            var senderAgent = await _context.Agents.Include(a => a.Agency).FirstOrDefaultAsync(a => a.UserId == sendingUserId);
            if (senderAgent?.Agency == null) return (false, "Нямате агенция.");

            var player = await _context.Players
                .Include(p => p.Agency)
                .Include(p => p.RepresentationContracts)
                .FirstOrDefaultAsync(p => p.Id == offerDto.TargetPlayerId);

            if (player == null) return (false, "Играчът не съществува.");
            if (player.AgencyId == null) return (false, "Този играч няма агент. Използвайте Pitch Player.");
            if (player.AgencyId == senderAgent.Agency.Id) return (false, "Това вече е ваш клиент!");

            var hasPendingOffer = await _context.Messages.AnyAsync(m =>
        m.SenderType == EntityType.Agency &&
        m.SenderId == senderAgent.Agency.Id &&
        m.RelatedEntityId == player.Id &&
        !m.IsActioned && // Ако съобщението не е приключено/отговорено
        (m.Type == MessageType.TransferOffer || m.Type == MessageType.ContractNegotiation)
    );

            if (hasPendingOffer)
            {
                return (false, "Вече сте изпратили оферта или водите преговори за този играч. Моля, изчакайте отговор!");
            }

            if (senderAgent.Agency.Budget < offerDto.OfferedAmount) return (false, "Нямате достатъчно бюджет за тази оферта.");

            // =========================================================
            // АВТОМАТИЧНО ПРИЕМАНЕ ЧРЕЗ RELEASE CLAUSE (ФИКС)
            // =========================================================
            var activeContract = player.RepresentationContracts.FirstOrDefault(c => c.IsActive);
            if (activeContract != null && activeContract.AgencyReleaseClause > 0 && offerDto.OfferedAmount >= activeContract.AgencyReleaseClause)
            {
                // 1. Създаваме съобщение до КУПУВАЧА (теб), за да предложиш договор на играча
                var contractMessage = new Message
                {
                    ReceiverAgencyId = senderAgent.Agency.Id,
                    SenderType = EntityType.Agency,
                    SenderId = player.AgencyId.Value,
                    SenderName = player.Agency.Name,
                    Subject = "Откупната Клауза е Активирана",
                    Content = $"Активирахте откупната клауза от ${offerDto.OfferedAmount:N0} за {player.Name}. Сега трябва да договорите личните условия с играча. Ако той откаже, трансферът се анулира.",
                    Type = MessageType.ContractNegotiation,
                    RelatedEntityId = player.Id,
                    DataValue = offerDto.OfferedAmount, // Сумата, която ще се плати, АКО играчът приеме
                    SentAt = DateTime.UtcNow
                };

                _context.Messages.Add(contractMessage);

                // 2. Пращаме информативно съобщение на СТАРАТА АГЕНЦИЯ, че някой е активирал клаузата
                await _messageService.SendMessageAsync(
                    player.AgencyId.Value,
                    EntityType.Agency,
                    senderAgent.Agency.Id,
                    senderAgent.Agency.Name,
                    "Активирана Откупна Клауза!",
                    $"Агенция {senderAgent.Agency.Name} активира откупната клауза от ${offerDto.OfferedAmount:N0} за {player.Name}. В момента те преговарят с играча за личните му условия.",
                    MessageType.Info);

                await _context.SaveChangesAsync();

                return (true, $"Откупната клауза е активирана! Отидете в Inbox, за да предложите лични условия на играча.");
            }

            // АКО СУМАТА Е ПО-МАЛКА ОТ КЛАУЗАТА -> ПРАЩАМЕ НОРМАЛНА ОФЕРТА:
            var placeholders = new Dictionary<string, string>
            {
                { "SenderName", senderAgent.Agency.Name },
                { "ReceiverName", player.Agency!.Name },
                { "PlayerName", player.Name },
                { "OfferAmount", offerDto.OfferedAmount.ToString("N0") }
            };

            var message = await _messageService.SendTemplatedMessageAsync(
                receiverAgencyId: player.AgencyId,
                senderType: EntityType.Agency,
                senderId: senderAgent.Agency.Id,
                senderName: senderAgent.Agency.Name,
                type: MessageType.TransferOffer,
                placeholders: placeholders,
                relatedEntityId: player.Id
            );

            message.DataValue = offerDto.OfferedAmount;
            await _context.SaveChangesAsync();

            return (true, $"Оферта от ${offerDto.OfferedAmount:N0} беше изпратена до агенцията на играча ({player.Agency.Name}).");
        }

        public async Task<(bool Success, string Message)> RespondToPoachOfferAsync(int respondingUserId, RespondToMessageOfferDto responseDto)
        {
            var responderAgent = await _context.Agents.Include(a => a.Agency).FirstOrDefaultAsync(a => a.UserId == respondingUserId);
            if (responderAgent?.Agency == null) return (false, "Нямате агенция.");

            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == responseDto.MessageId && m.ReceiverAgencyId == responderAgent.Agency.Id);

            if (message == null) return (false, "Съобщението не е намерено.");
            if (message.Type != MessageType.TransferOffer) return (false, "Това съобщение не е оферта.");
            if (message.RelatedEntityId == null) return (false, "Грешка в данните на офертата.");
            if (message.IsActioned) return (false, "Вече сте отговорили на тази оферта.");

            var targetPlayer = await _context.Players.FirstOrDefaultAsync(p => p.Id == message.RelatedEntityId.Value);
            if (targetPlayer == null) return (false, "Играчът вече не съществува.");

            // Определяме кой е купувач и кой продавач (защото при контра-оферта ролите се разменят)
            bool isResponderTheOwner = targetPlayer.AgencyId == responderAgent.Agency.Id;
            bool isSenderTheOwner = targetPlayer.AgencyId == message.SenderId;

            if (!isResponderTheOwner && !isSenderTheOwner) return (false, "Нито една от двете агенции не притежава правата на този играч.");

            int buyerAgencyId = isResponderTheOwner ? message.SenderId : responderAgent.Agency.Id;
            int sellerAgencyId = isResponderTheOwner ? responderAgent.Agency.Id : message.SenderId;
            string sellerAgencyName = isResponderTheOwner ? responderAgent.Agency.Name : message.SenderName;

            decimal offeredAmount = message.DataValue ?? 0;

            if (responseDto.IsAccepted)
            {
                var buyerAgency = await _context.Agencies.FirstOrDefaultAsync(a => a.Id == buyerAgencyId);
                if (buyerAgency == null) return (false, "Агенцията купувач вече не съществува.");

                if (buyerAgency.Budget < offeredAmount)
                {
                    message.IsActioned = true;
                    await _context.SaveChangesAsync();
                    return (false, "Трансферът пропадна. Агенцията купувач вече не разполага с нужните средства.");
                }

                // СТЪПКА 1 ПРИКЛЮЧИ УСПЕШНО: Създаваме съобщение на купувача да предложи договор на играча
                var contractMessage = new Message
                {
                    ReceiverAgencyId = buyerAgencyId,
                    SenderType = EntityType.Agency,
                    SenderId = sellerAgencyId,
                    SenderName = sellerAgencyName,
                    Subject = "Договорена Сума за Трансфер",
                    Content = $"Агенцията прие сумата от ${offeredAmount:N0} за {targetPlayer.Name}. Сега трябва да договорите личните условия с играча. Ако той откаже, трансферът се анулира.",
                    Type = MessageType.ContractNegotiation,
                    RelatedEntityId = targetPlayer.Id,
                    DataValue = offeredAmount,
                    SentAt = DateTime.UtcNow
                };

                _context.Messages.Add(contractMessage);
                message.IsActioned = true;
                await _context.SaveChangesAsync();

                return (true, "Трансферната сума е договорена! Очаква се купувачът да подпише личен договор с играча.");
            }
            else if (responseDto.CounterAmount.HasValue && responseDto.CounterAmount.Value > 0)
            {
                int receiverAgencyId = isResponderTheOwner ? buyerAgencyId : sellerAgencyId;

                var counterMessage = new Message
                {
                    ReceiverAgencyId = receiverAgencyId,
                    SenderType = EntityType.Agency,
                    SenderId = responderAgent.Agency.Id,
                    SenderName = responderAgent.Agency.Name,
                    Subject = "Контра-оферта за Трансфер",
                    Content = $"Предлагаме ви нова сума за правата на играча.",
                    Type = MessageType.TransferOffer,
                    RelatedEntityId = targetPlayer.Id,
                    DataValue = responseDto.CounterAmount.Value,
                    SentAt = DateTime.UtcNow
                };

                _context.Messages.Add(counterMessage);
                message.IsActioned = true;
                await _context.SaveChangesAsync();

                return (true, $"Изпратихте контра-оферта от ${responseDto.CounterAmount.Value:N0}.");
            }
            else
            {
                message.IsActioned = true;
                await _context.SaveChangesAsync();
                return (true, "Вие отхвърлихте офертата.");
            }
        }
    }
}