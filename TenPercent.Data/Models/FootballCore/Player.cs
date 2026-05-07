namespace TenPercent.Data.Models
{
    using System;
    using System.Collections.Generic;

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

        public decimal Balance { get; set; } = 0m;

        public int? ClubId { get; set; }
        public Club? Club { get; set; }

        public int? AgencyId { get; set; }
        public Agency? Agency { get; set; }

        public PlayerAttributes Attributes { get; set; } = null!;

        public ICollection<ClubContract> ClubContracts { get; set; } = new List<ClubContract>();
        public ICollection<RepresentationContract> RepresentationContracts { get; set; } = new List<RepresentationContract>();

        public ICollection<PlayerSeasonPerformance> SeasonPerformances { get; set; } = new List<PlayerSeasonPerformance>();
        public ICollection<PlayerMatchPerformance> MatchPerformances { get; set; } = new List<PlayerMatchPerformance>();
        public ICollection<AgencyShortlist> ShortlistedBy { get; set; } = new List<AgencyShortlist>();

        public void RecalculateCurrentAbility()
        {
            if (Position == null || Attributes == null) return;

            decimal ovr =
                (Attributes.Pace * Position.PaceWeight) +
                (Attributes.Shooting * Position.ShootingWeight) +
                (Attributes.Passing * Position.PassingWeight) +
                (Attributes.Dribbling * Position.DribblingWeight) +
                (Attributes.Defending * Position.DefendingWeight) +
                (Attributes.Physical * Position.PhysicalWeight);

            CurrentAbility = (int)Math.Round(ovr, 0);
        }
    }
}