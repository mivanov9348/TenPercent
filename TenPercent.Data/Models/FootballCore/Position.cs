namespace TenPercent.Data.Models
{    public class Position
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Abbreviation { get; set; } = string.Empty;
        public decimal PaceWeight { get; set; }
        public decimal ShootingWeight { get; set; }
        public decimal PassingWeight { get; set; }
        public decimal DribblingWeight { get; set; }
        public decimal DefendingWeight { get; set; }
        public decimal PhysicalWeight { get; set; }
        public decimal GoalkeepingWeight { get; set; }
        public decimal VisionWeight { get; set; }
        public decimal StaminaWeight { get; set; }
        public ICollection<Player> Players { get; set; } = new List<Player>();
    }
}