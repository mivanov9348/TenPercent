namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using TenPercent.Api.DTOs;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.Enums;
    using TenPercent.Data.Models;

    public class PlayerContractService : IPlayerContractService
    {
        private readonly AppDbContext _context;
        private readonly IFinanceService _financeService; // Ще ни трябва за Signing Bonus
        private readonly Random _rand = new Random();

        public PlayerContractService(AppDbContext context, IFinanceService financeService)
        {
            _context = context;
            _financeService = financeService;
        }

        // --- 1. ПЪРВОНАЧАЛНИ (ПОСТНИ) ДОГОВОРИ ЗА AI ---
        public async Task<bool> GenerateInitialContractsAsync()
        {
            var clubs = await _context.Clubs
                .Include(c => c.Players)
                .ThenInclude(p => p.ClubContracts)
                .Where(c => c.Players.Any())
                .ToListAsync();

            var newContracts = new List<ClubContract>();

            foreach (var club in clubs)
            {
                var playersNeedingContracts = club.Players
                    .Where(p => !p.ClubContracts.Any(cc => cc.IsActive))
                    .ToList();

                if (!playersNeedingContracts.Any()) continue;

                decimal maxWeeklyWageBill = club.WageBudget / 40m;
                int totalSquadAbility = playersNeedingContracts.Sum(p => p.CurrentAbility);

                foreach (var player in playersNeedingContracts)
                {
                    decimal abilityShare = totalSquadAbility > 0 ? (decimal)player.CurrentAbility / totalSquadAbility : 0;
                    decimal weeklyWage = Math.Round(maxWeeklyWageBill * abilityShare, 0);

                    var contract = new ClubContract
                    {
                        PlayerId = player.Id,
                        ClubId = club.Id,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddYears(_rand.Next(1, 4)),

                        WeeklyWage = weeklyWage,
                        
                        // НУЛИРАМЕ ВСИЧКО ОСТАНАЛО - Само гола заплата за AI-а
                        SigningBonus = 0, 
                        AppearanceBonus = 0,
                        GoalBonus = 0,
                        CleanSheetBonus = 0,
                        ReleaseClause = 0,
                        IsActive = true
                    };

                    newContracts.Add(contract);
                }
            }

            if (newContracts.Any())
            {
                _context.ClubContracts.AddRange(newContracts);
                await _context.SaveChangesAsync();
            }

            return true;
        }

        // --- 2. ПРЕГОВОРИ КЛУБ-ИГРАЧ (От Агента) ---
        public async Task<(bool Success, string Message, bool Accepted)> NegotiateContractAsync(NegotiateClubContractDto dto)
        {
            var player = await _context.Players
                .Include(p => p.ClubContracts)
                .Include(p => p.RepresentationContracts)
                .FirstOrDefaultAsync(p => p.Id == dto.PlayerId);

            if (player == null) return (false, "Играчът не съществува.", false);

            var club = await _context.Clubs.FirstOrDefaultAsync(c => c.Id == dto.TargetClubId);
            if (club == null) return (false, "Избраният клуб не съществува.", false);

            // ==========================================
            // ЛОГИКА НА КЛУБА: ЩЕ ПРИЕМЕ ЛИ ОФЕРТАТА?
            // ==========================================
            
            // 1. Проверка на Бюджета: Клубът може ли да си позволи тези пари?
            decimal maxWeeklyWage = club.WageBudget / 40m;
            if (dto.WeeklyWage > maxWeeklyWage * 0.30m) // Клубът няма да даде повече от 30% от общия си таван на 1 човек
            {
                return (true, $"Клуб {club.Name} отказва: Исканата заплата от {dto.WeeklyWage:N0} е извън нашата структура.", false);
            }

            // Проверка за Signing Bonus кеш
            if (dto.SigningBonus > club.TransferBudget)
            {
                return (true, $"Клуб {club.Name} отказва: Нямаме толкова кеш за Signing Bonus.", false);
            }

            // 2. Преценка спрямо OVR на играча
            // Очакваната заплата според OVR
            decimal expectedWage = player.CurrentAbility * 1000m; 
            
            // Ако искаш твърде много пари спрямо уменията му (повече от 30% над пазарното)
            if (dto.WeeklyWage > expectedWage * 1.30m)
            {
                return (true, $"Клуб {club.Name} отказва: {player.Name} не струва толкова пари.", false);
            }

            // Ако искаш абсурдни бонуси (напр. гол бонус равен на заплатата)
            if (dto.GoalBonus > dto.WeeklyWage * 0.50m)
            {
                return (true, $"Клуб {club.Name} отказва: Исканите бонуси са нереалистично високи.", false);
            }

            // КЛУБЪТ ПРИЕМА ОФЕРТАТА!

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Плащане на Signing Bonus от Клуба към Играча
                if (dto.SigningBonus > 0)
                {
                    var paymentResult = await _financeService.ProcessTransactionAsync(
                        EntityType.Club, club.Id,
                        EntityType.Player, player.Id,
                        dto.SigningBonus,
                        TransactionCategory.SigningBonus,
                        $"Signing Bonus for {player.Name} from {club.Name}"
                    );

                    if (!paymentResult.Success) throw new Exception("Грешка при плащането на бонуса от клуба.");

                    // ==========================================
                    // НОВО: АГЕНЦИЯТА СИ ПРИБИРА ПРОЦЕНТА ВЕДНАГА!
                    // ==========================================
                    var repContract = player.RepresentationContracts.FirstOrDefault(c => c.IsActive);
                    if (repContract != null && repContract.WageCommissionPercentage > 0)
                    {
                        // Изчисляваме твоя дял (напр. 10% от 1,000,000 = 100,000)
                        decimal agencyCut = Math.Round(dto.SigningBonus * (repContract.WageCommissionPercentage / 100m), 2);

                        if (agencyCut > 0)
                        {
                            var commissionResult = await _financeService.ProcessTransactionAsync(
                                EntityType.Player, player.Id,           // Играчът ти ги дава
                                EntityType.Agency, repContract.AgencyId,// Твоята агенция ги получава
                                agencyCut,
                                TransactionCategory.Commission,
                                $"Agency cut ({repContract.WageCommissionPercentage}%) from {player.Name}'s Signing Bonus"
                            );

                            if (!commissionResult.Success) throw new Exception("Грешка при превода на комисионната към Агенцията.");
                        }
                    }
                }

                // 2. Деактивиране на стария договор
                var oldContract = player.ClubContracts.FirstOrDefault(c => c.IsActive);
                if (oldContract != null)
                {
                    oldContract.IsActive = false;
                }

                // 3. Създаване на новия договор с всички екстри
                var newContract = new ClubContract
                {
                    PlayerId = player.Id,
                    ClubId = club.Id,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddYears(dto.DurationYears),

                    WeeklyWage = dto.WeeklyWage,
                    SigningBonus = dto.SigningBonus,
                    AppearanceBonus = dto.AppearanceBonus,
                    GoalBonus = dto.GoalBonus,
                    CleanSheetBonus = dto.CleanSheetBonus,
                    ReleaseClause = dto.ReleaseClause,

                    IsActive = true
                };

                _context.ClubContracts.Add(newContract);
                player.ClubId = club.Id; // Официално му сменяме отбора

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Успех! {club.Name} прие условията и подписа с {player.Name}.", true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Грешка при подписване: " + ex.Message, false);
            }
        }
    }
}