namespace TenPercent.Application.DTOs
{
    using System;
    using System.Collections.Generic;

    public class AgencyFinanceDto
    {
        public decimal Balance { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal NetProfit => TotalIncome - TotalExpenses;

        public List<TransactionDto> RecentTransactions { get; set; } = new List<TransactionDto>();
    }

}