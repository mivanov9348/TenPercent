namespace TenPercent.Data.Models
{
    using System;
    using System.Collections.Generic;

    public class Agency
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int LogoId { get; set; }

        public decimal Budget { get; set; } = 0m;

        public int Reputation { get; set; } = 0;
        public int Level { get; set; } = 1;
        public DateTime EstablishedAt { get; set; } = DateTime.UtcNow;

        public int AgentId { get; set; }
        public Agent Agent { get; set; } = null!;

        public ICollection<Player> Players { get; set; } = new List<Player>();
    }
}