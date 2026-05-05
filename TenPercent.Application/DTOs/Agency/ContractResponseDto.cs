namespace TenPercent.Application.DTOs
{  
    public class ContractResponseDto
    {
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public decimal? CounterWageCommission { get; set; }
        public decimal? CounterTransferCommission { get; set; }
        public decimal? CounterSigningBonus { get; set; }
    }
}