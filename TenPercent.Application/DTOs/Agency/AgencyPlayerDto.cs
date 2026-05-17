namespace TenPercent.Application.DTOs
{
    public class AgencyPlayerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Pos { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public string ClubName { get; set; } = string.Empty;

        public int Skill { get; set; } // OVR
        public decimal Value { get; set; } // MarketValue
        public decimal Wage { get; set; } // WeeklyWage

        // --- ДОГОВОРИ (Оставащи Сезони) ---
        public int ClubContractYearsLeft { get; set; }
        public int AgencyContractYearsLeft { get; set; }

        // --- АТРИБУТИ ---
        public int Pace { get; set; }
        public int Shooting { get; set; }
        public int Passing { get; set; }
        public int Dribbling { get; set; }
        public int Defending { get; set; }
        public int Physical { get; set; }
        public int Goalkeeping { get; set; }
        public int Vision { get; set; }
        public int Stamina { get; set; }
        public int Apps { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public decimal AvgRating { get; set; }
    }
}