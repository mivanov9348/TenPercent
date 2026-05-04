namespace TenPercent.Api.DTOs
{
    using System.Collections.Generic;

    public class ScoutingPlayerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public int Age { get; set; }
        public string Nationality { get; set; }
        public decimal MarketValue { get; set; }
        public string ClubName { get; set; }
        public bool HasAgency { get; set; }
    }

}