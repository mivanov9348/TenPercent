namespace TenPercent.Data.Models
{
    public class EconomySettings
    {
        public int Id { get; set; }

        // Агенция
        public decimal AgencyStartupGrant { get; set; } = 1_000_000m;
        public decimal AgencyIncomeTaxRate { get; set; } = 0.10m; // 10%

        // Банка
        public decimal InitialBankReserve { get; set; } = 100_000_000_000m;

        // Клубове (Формула: Base + (Reputation * Multiplier))
        public decimal ClubBaseGrant { get; set; } = 10_000_000m;
        public decimal ClubReputationMultiplier { get; set; } = 1_000_000m;
        public decimal ClubWageBudgetPercentage { get; set; } = 0.60m; // 60%
    }
}