namespace TenPercent.Data.Models
{
    public class Season
    {
        public int Id { get; set; }
        public int SeasonNumber { get; set; } 

        public int CurrentGameweek { get; set; } = 1;
        public int TotalGameweeks { get; set; } = 0; 

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }  
        public bool IsActive { get; set; } = true; 

        public ICollection<Fixture> Fixtures { get; set; } = new List<Fixture>();
        public ICollection<SeasonStanding> Standings { get; set; } = new List<SeasonStanding>();
        public ICollection<PlayerSeasonPerformance> PlayerStats { get; set; } = new List<PlayerSeasonPerformance>();
    }
}