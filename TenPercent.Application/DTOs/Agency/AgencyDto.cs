namespace TenPercent.Application.DTOs
{
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
    }
}