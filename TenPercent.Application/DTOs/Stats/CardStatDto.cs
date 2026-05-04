namespace TenPercent.Api.DTOs
{
    using System.Collections.Generic;

   

    public class CardStatDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Club { get; set; }
        public int Yellow { get; set; }
        public int Red { get; set; }
        public int Matches { get; set; }
    }

}