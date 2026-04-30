namespace TenPercent.Data.Models
{
    public class Club
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public int LeagueId { get; set; }
        public League League { get; set; } = null!;
        public string PrimaryColor { get; set; } = string.Empty;

        public int Reputation { get; set; } 
        public int Level { get; set; } 

        public decimal TransferBudget { get; set; }
        public decimal WageBudget { get; set; } 

        public ICollection<Player> Players { get; set; } = new List<Player>();
    }
}