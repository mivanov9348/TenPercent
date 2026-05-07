namespace TenPercent.Data.Models
{
    public class ClubContract
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;
        public int ClubId { get; set; }
        public Club Club { get; set; } = null!;

        public int StartSeasonNumber { get; set; }
        public int EndSeasonNumber { get; set; }

        public decimal WeeklyWage { get; set; }
        public decimal SigningBonus { get; set; }
        public decimal ReleaseClause { get; set; }

        public decimal AppearanceBonus { get; set; }
        public decimal GoalBonus { get; set; }
        public decimal AssistBonus { get; set; }
        public decimal CleanSheetBonus { get; set; }

        public int GoalMilestoneTarget { get; set; }
        public decimal GoalMilestoneBonus { get; set; }

        public bool IsActive { get; set; } = true;
    }
}