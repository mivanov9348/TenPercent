namespace TenPercent.Application.DTOs
{
    using System;
    using System.Collections.Generic;   

    public class TransactionDto
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; 
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
    }
}