namespace TenPercent.Data.Models
{
    using System;
    public class RepresentationContract
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public int AgencyId { get; set; }
        public Agency Agency { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // --- КЛАУЗИ И КОМИСИОННИ ---

        // Процент от седмичната му заплата (напр. 10%) - редовен доход
        public decimal WageCommissionPercentage { get; set; }

        // Процент от трансферната сума, когато бъде продаден (напр. 5%) - големият удар
        public decimal TransferCommissionPercentage { get; set; }

        // Пари, които Агенцията е платила на играча "на ръка", за да подпише с нея (твой разход)
        public decimal SigningBonusPaid { get; set; }

        // Неустойка: Ако друга агенция иска да ти го открадне преди края на договора, трябва да ти плати това
        public decimal AgencyReleaseClause { get; set; }

        public bool IsActive { get; set; } = true;
    }
}