namespace TenPercent.Application.DTOs
{
    public class PlayerMatchDto
    {
        public int Gameweek { get; set; }
        public DateTime MatchDate { get; set; }
        public string OpponentName { get; set; } = string.Empty;
        public bool IsHomeMatch { get; set; }
        public int MinutesPlayed { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public decimal Rating { get; set; }
    }
}
