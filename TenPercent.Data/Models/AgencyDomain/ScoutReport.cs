namespace TenPercent.Data.Models
{
    using System;
    public class ScoutReport
    {
        public int Id { get; set; }

        // Връзка към играча, за когото е докладът
        public int PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        // Връзка към агенцията, която го е поръчала (и платила)
        public int AgencyId { get; set; }
        public Agency Agency { get; set; } = null!;

        // Дата на създаване/обновяване (Докладите остаряват!)
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        // Колко подробен е докладът (1 = Базов, 5 = Пълно разкриване)
        // Зависи от нивото на "Scouting Network" инфраструктурата на агенцията
        public int KnowledgeLevel { get; set; }

        // --- ДАННИТЕ В ДОКЛАДА (Какво "вижда" скаутът) ---

        // Диапазон за текущото умение (напр. ако е 82 OVR, скаут с ниво 2 може да каже 78-85)
        public int MinEstimatedOVR { get; set; }
        public int MaxEstimatedOVR { get; set; }

        // Диапазон за потенциала
        public int MinEstimatedPOT { get; set; }
        public int MaxEstimatedPOT { get; set; }

        // Препоръка от скаута (напр. "Sign Immediately", "Avoid", "Monitor")
        public string RecommendationGrade { get; set; } = string.Empty;

        // Текстово описание на силни и слаби страни
        public string Strengths { get; set; } = string.Empty;
        public string Weaknesses { get; set; } = string.Empty;

        // Доклад за характера (Опит за разгадаване на Greed/Ambition)
        public string PersonalityNotes { get; set; } = string.Empty;

        // Очаквана пазарна цена (може да варира от реалната)
        public decimal EstimatedMarketValue { get; set; }
        public decimal EstimatedWageDemand { get; set; }
    }
}