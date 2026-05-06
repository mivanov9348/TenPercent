namespace TenPercent.Data.DTOs.Admin
{
    using System.Collections.Generic;
    using TenPercent.Data.Models.Finance;

    public class BankDashboardDto
    {
        public decimal ReserveBalance { get; set; }
        public decimal TotalTaxesCollected { get; set; }
        public decimal TotalGrantsGiven { get; set; }
        public decimal MoneyInCirculation { get; set; } 
        public List<Transaction> RecentTransactions { get; set; } = new List<Transaction>();
    }
}