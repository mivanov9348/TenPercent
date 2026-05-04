namespace TenPercent.Api.DTOs
{
    using System.Collections.Generic;

   
    public class SeasonStatsDto
    {
        public List<TopStatDto> TopScorers { get; set; } = new();
        public List<TopStatDto> TopRatings { get; set; } = new();
        public List<TopStatDto> TopAssists { get; set; } = new();
        public List<CardStatDto> MostCards { get; set; } = new();
    }
}