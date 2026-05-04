namespace TenPercent.Api.DTOs
{
    using System.Collections.Generic;

    public class TopStatDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Club { get; set; }
        public decimal Value { get; set; } // Използваме decimal, за да побере както голове (int), така и рейтинг (double)
        public int Matches { get; set; }
    }

}