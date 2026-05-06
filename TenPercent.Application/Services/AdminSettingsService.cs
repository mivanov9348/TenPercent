namespace TenPercent.Application.Services
{
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;
    using TenPercent.Application.Services.Interfaces;
    using TenPercent.Data;
    using TenPercent.Data.DTOs.Admin;
    using TenPercent.Data.Models;

    public class AdminSettingsService : IAdminSettingsService
    {
        private readonly AppDbContext _context;

        public AdminSettingsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<EconomySettingsDto> GetSettingsAsync()
        {
            var settings = await _context.EconomySettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new EconomySettings();
                _context.EconomySettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return new EconomySettingsDto
            {
                Id = settings.Id,
                AgencyStartupGrant = settings.AgencyStartupGrant,
                AgencyIncomeTaxRate = settings.AgencyIncomeTaxRate,
                InitialBankReserve = settings.InitialBankReserve,
                ClubBaseGrant = settings.ClubBaseGrant,
                ClubReputationMultiplier = settings.ClubReputationMultiplier,
                ClubWageBudgetPercentage = settings.ClubWageBudgetPercentage,
                GlobalIncomeTax = settings.GlobalIncomeTax
            };
        }

        public async Task<(bool Success, string Message)> UpdateSettingsAsync(EconomySettingsDto dto)
        {
            var settings = await _context.EconomySettings.FirstOrDefaultAsync();
            if (settings == null) return (false, "Настройките не са намерени в базата.");

            settings.AgencyStartupGrant = dto.AgencyStartupGrant;
            settings.AgencyIncomeTaxRate = dto.AgencyIncomeTaxRate;
            settings.InitialBankReserve = dto.InitialBankReserve;
            settings.ClubBaseGrant = dto.ClubBaseGrant;
            settings.ClubReputationMultiplier = dto.ClubReputationMultiplier;
            settings.ClubWageBudgetPercentage = dto.ClubWageBudgetPercentage;
            settings.GlobalIncomeTax = dto.GlobalIncomeTax;

            _context.EconomySettings.Update(settings);
            await _context.SaveChangesAsync();

            return (true, "Икономическите настройки са обновени успешно!");
        }
    }
}