namespace TenPercent.Data.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Position { get; set; } = string.Empty; // ST, MID, DEF, GK
        public string Nationality { get; set; } = string.Empty;

        // Основни Атрибути
        public int Overall { get; set; }
        public int Potential { get; set; }
        public int Pace { get; set; }
        public int Shooting { get; set; }
        public int Passing { get; set; }
        public int Dribbling { get; set; }
        public int Defending { get; set; }
        public int Physical { get; set; }

        // Финанси и Договори
        public decimal MarketValue { get; set; }
        public decimal WeeklyWage { get; set; }
        public int ContractYearsLeft { get; set; }
        public string Form { get; set; } = "Good";

        // Връзки (Relations)
        public int? ClubId { get; set; } // Може да е null (Свободен агент)
        public Club? Club { get; set; }

        public int? AgencyId { get; set; } // Може да е null (Без агент)
        public Agency? Agency { get; set; }
    }
}