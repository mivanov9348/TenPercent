namespace TenPercent.Api.DTOs
{
    public class ScoutingPlayerDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Nationality { get; set; } = string.Empty;
        public decimal MarketValue { get; set; }
        public string ClubName { get; set; } = string.Empty;

        public bool HasAgency { get; set; }

        // 🔥 ЕТО ГО НОВОТО ПОЛЕ 🔥
        public string? AgencyName { get; set; }

        // --- FM STYLE ATTRIBUTES ---
        public string Pace { get; set; } = string.Empty;
        public string Shooting { get; set; } = string.Empty;
        public string Passing { get; set; } = string.Empty;
        public string Dribbling { get; set; } = string.Empty;
        public string Defending { get; set; } = string.Empty;
        public string Physical { get; set; } = string.Empty;

        // --- СЕЗОННИ СТАТИСТИКИ (НОВО) ---
        public int Apps { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public decimal AvgRating { get; set; }

        public decimal WeeklyWage { get; set; }
    }
}