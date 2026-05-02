namespace TenPercent.Data.Models
{
    public class WorldState
    {
        public int Id { get; set; }
        public int? CurrentSeasonId { get; set; }
        public bool IsSimulationRunning { get; set; } = false;
        public DateTime? NextMatchdayDate { get; set; }

    }
}