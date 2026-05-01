namespace TenPercent.Data.Models
{
    public class PlayerSeasonPerformance
    {
        public int Id { get; set; }

        public int SeasonId { get; set; }
        public Season Season { get; set; } = null!;

        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public int? ClubId { get; set; } 
        public Club? Club { get; set; }

        public int Appearances { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }
        public decimal AverageRating { get; set; }
    }
}