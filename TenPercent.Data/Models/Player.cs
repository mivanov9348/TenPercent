namespace TenPercent.Data.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Position { get; set; } = string.Empty; // ST, MID, DEF, GK
        public string Nationality { get; set; } = string.Empty;

        // --- ДВИГАТЕЛ НА ИГРАТА (Скрити за всички! Използват се за математиката на играта) ---
        // Това е реалният Overall, но никой никога не го вижда директно в UI-а.
        public int CurrentAbility { get; set; }   // CA: 1-100 
        public int PotentialAbility { get; set; } // PA: 1-100 

        // --- ФИНАНСИ И ДОГОВОРИ ---
        public decimal MarketValue { get; set; }
        public decimal WeeklyWage { get; set; }
        public int ContractYearsLeft { get; set; }
        public string Form { get; set; } = "Good";

        // --- ВРЪЗКИ (Relations) ---
        public int? ClubId { get; set; }
        public Club? Club { get; set; }

        public int? AgencyId { get; set; }
        public Agency? Agency { get; set; }

        // --- ДЕТАЙЛИ ---
        public PlayerAttributes Attributes { get; set; } = null!;
    }
}