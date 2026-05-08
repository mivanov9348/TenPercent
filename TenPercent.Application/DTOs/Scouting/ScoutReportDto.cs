namespace TenPercent.Application.DTOs
{
    using System;

    public class ScoutReportDto
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public DateTime GeneratedAt { get; set; }
        public int KnowledgeLevel { get; set; }

        public int MinOVR { get; set; }
        public int MaxOVR { get; set; }
        public int MinPOT { get; set; }
        public int MaxPOT { get; set; }

        public string RecommendationGrade { get; set; } = string.Empty;
        public string Strengths { get; set; } = string.Empty;
        public string Weaknesses { get; set; } = string.Empty;
        public string PersonalityNotes { get; set; } = string.Empty;

        public decimal EstimatedValue { get; set; }
        public decimal EstimatedWage { get; set; }
    }
}