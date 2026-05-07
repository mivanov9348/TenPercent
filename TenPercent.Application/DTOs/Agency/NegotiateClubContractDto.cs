namespace TenPercent.Api.DTOs
{
    public class NegotiateClubContractDto
    {
        public int PlayerId { get; set; }
        public int TargetClubId { get; set; } // Отборът, с който преговаряш

        public int DurationYears { get; set; }
        public decimal WeeklyWage { get; set; }
        public decimal SigningBonus { get; set; }
        public decimal AppearanceBonus { get; set; }
        public decimal GoalBonus { get; set; }
        public decimal CleanSheetBonus { get; set; }
        public decimal ReleaseClause { get; set; }

    }
}