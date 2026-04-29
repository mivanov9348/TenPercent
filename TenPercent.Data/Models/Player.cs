namespace TenPercent.Data.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Position { get; set; } = string.Empty;

        public int Overall { get; set; }
        public int Potential { get; set; }

        public decimal MarketValue { get; set; }
        public decimal WeeklyWage { get; set; }
        public int ContractYearsLeft { get; set; }
        public string Form { get; set; } = "Good"; 

        public int? AgencyId { get; set; }
        public Agency? Agency { get; set; }
    }
}