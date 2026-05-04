namespace TenPercent.Data.Models
{
    using System;

    public class ClubContract
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        public int ClubId { get; set; }
        public Club Club { get; set; } = null!;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // --- ФИНАНСОВИ ПАРАМЕТРИ ---

        // Базова заплата
        public decimal WeeklyWage { get; set; }

        // Бонус, платен от клуба на играча при подписване (Ти като агент можеш да вземеш % и от него в бъдеще)
        public decimal SigningBonus { get; set; }

        // Бонуси за представяне (Ще ги вържем с MatchEngine-а по-късно!)
        public decimal AppearanceBonus { get; set; } // Пари за всеки изигран мач
        public decimal GoalBonus { get; set; }       // Пари за всеки вкаран гол
        public decimal CleanSheetBonus { get; set; } // Пари за суха мрежа (за вратари и защитници)

        // Откупна клауза: Ако някой клуб плати тази сума, настоящият клуб не може да спре трансфера
        public decimal ReleaseClause { get; set; }

        public bool IsActive { get; set; } = true;
    }
}