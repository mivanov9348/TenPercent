namespace TenPercent.Application.DTOs
{
    using System;
    using System.Text.Json.Serialization;

    public class MessageDto
    {
        public int Id { get; set; }
        public string SenderName { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public string Type { get; set; }
        public int? RelatedEntityId { get; set; }
        public decimal? DataValue { get; set; }
        public bool IsActioned { get; set; }
        public string TargetPlayerName { get; set; }

        // ЗАДАВАМЕ ИЗРИЧНО ИМЕТО ЗА REACT
        [JsonPropertyName("currentContract")]
        public ContractInfoDto CurrentContract { get; set; }
    }
}