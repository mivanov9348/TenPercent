namespace TenPercent.Data.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Position { get; set; } = string.Empty; // ST, MID, DEF, GK
        public string Nationality { get; set; } = string.Empty;

        public int CurrentAbility { get; set; }   // CA: 1-100 
        public int PotentialAbility { get; set; } // PA: 1-100 

        public decimal MarketValue { get; set; }
        public decimal WeeklyWage { get; set; }
        public int ContractYearsLeft { get; set; }
        public string Form { get; set; } = "Good";

        public int? ClubId { get; set; }
        public Club? Club { get; set; }

        public int? AgencyId { get; set; }
        public Agency? Agency { get; set; }

        public PlayerAttributes Attributes { get; set; } = null!;
    }
}