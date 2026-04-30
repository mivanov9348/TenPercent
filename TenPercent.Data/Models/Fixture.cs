namespace TenPercent.Data.Models
{
    public class Fixture
    {
        public int Id { get; set; }
        public int Season { get; set; }
        public int Gameweek { get; set; }
        public DateTime ScheduledDate { get; set; } 

        public bool IsPlayed { get; set; } = false;

        public int HomeGoals { get; set; }
        public int AwayGoals { get; set; }

        public int LeagueId { get; set; }
        public League League { get; set; } = null!;

        public int HomeClubId { get; set; }
        public Club HomeClub { get; set; } = null!;

        public int AwayClubId { get; set; }
        public Club AwayClub { get; set; } = null!;

        public ICollection<PlayerPerformance> Performances { get; set; } = new List<PlayerPerformance>();
    }
}