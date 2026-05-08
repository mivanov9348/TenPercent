namespace TenPercent.Data.Models
{
    public class ScoutTemplate
    {
        public int Id { get; set; }

        // Категория на фразата: "Strength", "Weakness", "Personality", "Recommendation"
        public string Category { get; set; } = string.Empty;

        // Името на атрибута, който проверяваме (напр. "Pace", "Shooting", "Greed", "OVR")
        public string AttributeName { get; set; } = string.Empty;

        // Диапазон, в който тази фраза е валидна (напр. от 80 до 99)
        public int MinValue { get; set; }
        public int MaxValue { get; set; }

        // (Опционално) Ако искаме фразата да е само за защитници ("DEF") или вратари ("GK")
        // Ако е null, важи за всички позиции.
        public string? TargetPosition { get; set; }

        // Самият текст на скаута
        public string Text { get; set; } = string.Empty;
    }
}