namespace TenPercent.Data.Models
{
    public class PlayerPerformance
    {
        public int Id { get; set; }

        public int MinutesPlayed { get; set; }
        public decimal MatchRating { get; set; } 
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }
        public int InjuryDays { get; set; } 
        public int FixtureId { get; set; }
        public Fixture Fixture { get; set; } = null!;

        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;
    }
}