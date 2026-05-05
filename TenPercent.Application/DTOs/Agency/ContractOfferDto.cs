namespace TenPercent.Application.DTOs
{
    public class ContractOfferDto
    {
        public int PlayerId { get; set; }
        public decimal WageCommissionPercentage { get; set; } 
        public decimal TransferCommissionPercentage { get; set; } 
        public decimal SigningBonusPaid { get; set; } 
        public decimal AgencyReleaseClause { get; set; } 
        public int DurationYears { get; set; } = 2;
    }
    
}