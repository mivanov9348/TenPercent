namespace TenPercent.Application.DTOs
{
    using System.Collections.Generic;

    public class PlayerDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Nationality { get; set; }
        public string Position { get; set; }
        public int OVR { get; set; }
        public int POT { get; set; }

        public int Pace { get; set; }
        public int Shooting { get; set; }
        public int Passing { get; set; }
        public int Dribbling { get; set; }
        public int Defending { get; set; }
        public int Physical { get; set; }

        public int Ambition { get; set; }
        public int Greed { get; set; }
        public int Loyalty { get; set; }

        public decimal MarketValue { get; set; }
        public string Form { get; set; }
        public int? ClubId { get; set; }
        public string ClubName { get; set; }
        public string AgencyName { get; set; }

        public decimal WeeklyWage { get; set; }
        public int ContractYearsLeft { get; set; }
        public bool HasAgent { get; set; }

        public int SeasonAppearances { get; set; }
        public int SeasonGoals { get; set; }
        public int SeasonAssists { get; set; }
        public decimal SeasonAverageRating { get; set; }
        public int SeasonYellowCards { get; set; }
        public int SeasonRedCards { get; set; }

        // --- ИСТОРИЯ НА МАЧОВЕТЕ (Последните 5-10 мача) ---
        public List<PlayerMatchDto> RecentMatches { get; set; } = new List<PlayerMatchDto>();
    }
    
}