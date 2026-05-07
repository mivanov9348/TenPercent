namespace TenPercent.Data.Models.Finance
{
    using System;
    using TenPercent.Data.Enums;

    public class Transaction
    {
        public int Id { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow; 
        public int? SeasonId { get; set; }
        public Season? Season { get; set; }
        public decimal Amount { get; set; }
        public EntityType SenderType { get; set; }
        public int? SenderId { get; set; }
        public EntityType ReceiverType { get; set; }
        public int? ReceiverId { get; set; }
        public TransactionCategory Category { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}