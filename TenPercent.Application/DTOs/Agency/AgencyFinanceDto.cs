namespace TenPercent.Application.DTOs
{
    using System.Collections.Generic;

    public class AgencyFinanceDto
    {
        public decimal Balance { get; set; }
        public decimal StartupCapital { get; set; } 

        public decimal OperatingIncome { get; set; }
        public decimal OperatingExpenses { get; set; }
        public decimal NetProfit => OperatingIncome - OperatingExpenses;

        public List<SeasonFinanceSummaryDto> SeasonalRecords { get; set; } = new();
        public List<TransactionDto> RecentTransactions { get; set; } = new();
    }

}