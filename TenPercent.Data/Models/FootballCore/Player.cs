namespace TenPercent.Data.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Nationality { get; set; } = string.Empty;

        public int PositionId { get; set; }
        public Position Position { get; set; } = null!;

        public int CurrentAbility { get; set; }
        public int PotentialAbility { get; set; }

        public decimal MarketValue { get; set; }
        public string Form { get; set; } = "Good";

        public int? ClubId { get; set; }
        public Club? Club { get; set; }

        public int? AgencyId { get; set; }
        public Agency? Agency { get; set; }

        public PlayerAttributes Attributes { get; set; } = null!;

        // --- НОВО: История на договорите ---
        public ICollection<ClubContract> ClubContracts { get; set; } = new List<ClubContract>();
        public ICollection<RepresentationContract> RepresentationContracts { get; set; } = new List<RepresentationContract>();
        
        public ICollection<PlayerSeasonPerformance> SeasonPerformances { get; set; } = new List<PlayerSeasonPerformance>();
        public ICollection<PlayerMatchPerformance> MatchPerformances { get; set; } = new List<PlayerMatchPerformance>();
        public ICollection<AgencyShortlist> ShortlistedBy { get; set; } = new List<AgencyShortlist>();

    }
}