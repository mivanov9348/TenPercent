namespace TenPercent.Data.Models
{
    using System;
    using TenPercent.Data.Enums;

    public class Message
    {
        public int Id { get; set; }

        public int? ReceiverAgencyId { get; set; }
        public Agency? ReceiverAgency { get; set; }

        public EntityType SenderType { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        public MessageType Type { get; set; }

        // ID на свързания обект (напр. ID на трансферната оферта) за да знаем какво да приемем/отхвърлим
        public int? RelatedEntityId { get; set; }

        // За да пазим предложената сума при оферти
        public decimal? DataValue { get; set; }

        // За да знаем дали вече е кликнато "Accept/Reject" и да скрием бутоните
        public bool IsActioned { get; set; } = false;
    }
}