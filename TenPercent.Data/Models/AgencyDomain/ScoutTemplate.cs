namespace TenPercent.Data.Models
{
    using TenPercent.Data.Enums.TenPercent.Data.Models.Enums;
    public class ScoutTemplate
    {
        public int Id { get; set; }

        // Тук вече използваш твоя Enum вместо string
        public ScoutCategory Category { get; set; }

        public string AttributeName { get; set; } = string.Empty;

        public int MinValue { get; set; }
        public int MaxValue { get; set; }

        public string? TargetPosition { get; set; }

        public string Text { get; set; } = string.Empty;
    }
}