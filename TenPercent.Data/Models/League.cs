namespace TenPercent.Data.Models
{
    public class League
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int Reputation { get; set; }

        public ICollection<Club> Clubs { get; set; } = new List<Club>();
        public ICollection<Fixture> Fixtures { get; set; } = new List<Fixture>();
    }
}