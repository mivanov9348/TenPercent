namespace TenPercent.Application.DTOs
{
    using System;
    using System.Text.Json.Serialization;

    public class ContractInfoDto
    {
        [JsonPropertyName("playerName")]
        public string PlayerName { get; set; }

        [JsonPropertyName("wageCommission")]
        public decimal WageCommission { get; set; }

        [JsonPropertyName("transferCommission")]
        public decimal TransferCommission { get; set; }

        [JsonPropertyName("releaseClause")]
        public decimal? ReleaseClause { get; set; }

        [JsonPropertyName("endSeasonNumber")]
        public int EndSeasonNumber { get; set; }
    }

}