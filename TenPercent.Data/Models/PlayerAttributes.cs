namespace TenPercent.Data.Models
{
    public class PlayerAttributes
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;
        public int Pace { get; set; }
        public int Shooting { get; set; }
        public int Passing { get; set; }
        public int Dribbling { get; set; }
        public int Defending { get; set; }
        public int Physical { get; set; }

        // --- ПОТЕНЦИАЛНИ АТРИБУТИ (Таванът на развитие) ---
        public int PotentialPace { get; set; }
        public int PotentialShooting { get; set; }
        public int PotentialPassing { get; set; }
        public int PotentialDribbling { get; set; }
        public int PotentialDefending { get; set; }
        public int PotentialPhysical { get; set; }

        // --- ПЕРСОНАЛНОСТ (Скрити - определят поведението и събитията) ---
        public int Ambition { get; set; } // Иска трансфери в топ клубове
        public int Greed { get; set; }    // Иска огромни заплати и бонуси
        public int Loyalty { get; set; }  // Трудно си сменя агента/клуба

        // Опционално за бъдещето:
        public int InjuryProne { get; set; } // Предразположеност към контузии (1-100)
    }
}