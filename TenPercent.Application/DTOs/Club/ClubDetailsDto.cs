namespace TenPercent.Api.DTOs
{   
    public class ClubDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string LeagueName { get; set; }
        public string PrimaryColor { get; set; }
        public int Reputation { get; set; }
        public decimal TransferBudget { get; set; }
        public decimal WageBudget { get; set; }
        public ClubSquadDto Squad { get; set; }
    }
}