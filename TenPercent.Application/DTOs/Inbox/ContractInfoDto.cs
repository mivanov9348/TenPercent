namespace TenPercent.Application.DTOs
{
    using System;

    public class ContractInfoDto
    {
        public string PlayerName { get; set; }
        public decimal WageCommission { get; set; }
        public decimal TransferCommission { get; set; }
        public decimal? ReleaseClause { get; set; }
        public int EndSeasonNumber { get; set; }
    }

}