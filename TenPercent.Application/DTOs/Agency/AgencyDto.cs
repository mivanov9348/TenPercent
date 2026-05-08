namespace TenPercent.Application.DTOs
{
    using System;

    public class AgencyDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;
        public int LogoId { get; set; }
        public decimal Budget { get; set; }
        public int Reputation { get; set; }
        public int Level { get; set; }
        public DateTime EstablishedAt { get; set; }
        public int TotalPlayersCount { get; set; }
        public decimal ProjectedSeasonIncome { get; set; }
        public string TopEarnerName { get; set; } = "N/A";
        public decimal TotalContractsValue { get; set; }
    }
}