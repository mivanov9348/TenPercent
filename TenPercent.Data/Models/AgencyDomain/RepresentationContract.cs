namespace TenPercent.Data.Models
{
    public class RepresentationContract
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;
        public int AgencyId { get; set; }
        public Agency Agency { get; set; } = null!;

        public int StartSeasonNumber { get; set; }
        public int EndSeasonNumber { get; set; }

        public decimal IncomeCommissionPercentage { get; set; }
        public decimal TransferCommissionPercentage { get; set; }
        public decimal AgencyBrokerFee { get; set; }
        public decimal SigningBonusPaid { get; set; }
        public decimal AgencyReleaseClause { get; set; }

        public bool IsActive { get; set; } = true;
    }
}