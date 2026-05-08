namespace TenPercent.Application.DTOs
{
    public class SeasonFinanceSummaryDto
    {
        public int SeasonId { get; set; }
        public int SeasonNumber { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }
        public decimal Profit => Income - Expenses;
    }
}
