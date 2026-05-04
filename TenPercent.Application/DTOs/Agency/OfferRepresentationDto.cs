namespace TenPercent.Application.DTOs
{
    public class OfferRepresentationDto
    {
        public int PlayerId { get; set; }

        // Времетраене
        public int DurationYears { get; set; }

        // Твоите комисионни
        public decimal WageCommissionPercentage { get; set; }
        public decimal TransferCommissionPercentage { get; set; }

        // Твой разход (Стимул за играча)
        public decimal SigningBonusPaid { get; set; }

        // Застраховка за теб
        public decimal AgencyReleaseClause { get; set; }
    }
}