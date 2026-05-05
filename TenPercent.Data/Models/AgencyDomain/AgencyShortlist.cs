namespace TenPercent.Data.Models
{
    using System;

    public class AgencyShortlist
    {
        public int AgencyId { get; set; }
        public Agency Agency { get; set; } = null!;

        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}