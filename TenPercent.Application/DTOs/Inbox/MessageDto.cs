namespace TenPercent.Application.DTOs
{
    using System;

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
        public ContractInfoDto CurrentContract { get; set; }
    }
}