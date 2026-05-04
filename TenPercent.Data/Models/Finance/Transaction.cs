namespace TenPercent.Data.Models.Finance
{
    using System;
    using TenPercent.Data.Enums;
    public class Transaction
    {
        public int Id { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow; // Игровата дата

        public decimal Amount { get; set; }

        public EntityType SenderType { get; set; }
        public int? SenderId { get; set; } // Null ако е Банката (или можем винаги да слагаме 1)

        public EntityType ReceiverType { get; set; }
        public int? ReceiverId { get; set; } // Ако е Играч, парите просто се "харчат" (излизат от играта)

        public TransactionCategory Category { get; set; }

        public string Description { get; set; } = string.Empty;
    }
}