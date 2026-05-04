namespace TenPercent.Data.Models.Finance
{
    using System;

    public class Bank
    {
        public int Id { get; set; }
        public string Name { get; set; } = "    ";

        public decimal ReserveBalance { get; set; }
    }
}