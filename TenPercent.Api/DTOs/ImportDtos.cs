namespace TenPercent.Api.DTOs
{
    public class LeagueImportDto
    {
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int Reputation { get; set; } // Make sure you add a Reputation column to your League Excel!
    }

    public class ClubImportDto
    {
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int LeagueId { get; set; }
        public string LeagueName { get; set; }
        public string PrimaryColor { get; set; } = string.Empty;
        public int Reputation { get; set; }
        public int Level { get; set; }
        public decimal TransferBudget { get; set; }
        public decimal WageBudget { get; set; }
    }
}