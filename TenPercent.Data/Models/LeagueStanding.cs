namespace TenPercent.Data.Models
{
    public class LeagueStanding
    {
        public int Id { get; set; }

        public int LeagueId { get; set; }
        public League League { get; set; } = null!;

        public int ClubId { get; set; }
        public Club Club { get; set; } = null!;

        public int Played { get; set; }
        public int Won { get; set; }
        public int Drawn { get; set; }
        public int Lost { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int Points { get; set; }
    }
}