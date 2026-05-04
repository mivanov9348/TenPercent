namespace TenPercent.Api.DTOs
{
    using System.Collections.Generic;

    public class ClubPlayerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public string Position { get; set; }
        public int Overall { get; set; }
        public int Potential { get; set; }
        public decimal MarketValue { get; set; }
    }

}