namespace TenPercent.Api.DTOs
{
    public class CreateAgencyDto
    {
        public int UserId { get; set; } 
        public string AgentName { get; set; } = string.Empty;
        public string AgencyName { get; set; } = string.Empty;
        public int LogoId { get; set; }
    }
}