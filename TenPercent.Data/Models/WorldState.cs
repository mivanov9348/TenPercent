namespace TenPercent.Data.Models
{
    public class WorldState
    {
        public int Id { get; set; }
        public int CurrentSeason { get; set; } = 1;
        public int CurrentGameweek { get; set; } = 1;
        public int TotalGameweeks { get; set; } = 18; 
        public DateTime NextMatchdayDate { get; set; } 
        public bool IsSimulationRunning { get; set; } = false;
    }
}