namespace TenPercent.Application.DTOs
{
    using System;
    using TenPercent.Data.Enums;

    public class MessageDto
    {
        public int Id { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public string Type { get; set; } = string.Empty; // Връщаме го като string за по-лесно четене в React
        public int? RelatedEntityId { get; set; }
    }
}