namespace TenPercent.Data.DTOs.Admin
{
    public class EconomySettingsDto
    {
        public int Id { get; set; }
        public decimal AgencyStartupGrant { get; set; }
        public decimal AgencyIncomeTaxRate { get; set; }
        public decimal InitialBankReserve { get; set; }
        public decimal ClubBaseGrant { get; set; }
        public decimal ClubReputationMultiplier { get; set; }
        public decimal ClubWageBudgetPercentage { get; set; }
        public decimal GlobalIncomeTax { get; set; }
    }
}
