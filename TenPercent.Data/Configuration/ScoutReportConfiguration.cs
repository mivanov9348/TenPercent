namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models;

    public class ScoutReportConfiguration : IEntityTypeConfiguration<ScoutReport>
    {
        public void Configure(EntityTypeBuilder<ScoutReport> builder)
        {
            builder.HasKey(sr => sr.Id);

            // Една агенция има само един доклад за даден играч (уникален индекс)
            builder.HasIndex(sr => new { sr.AgencyId, sr.PlayerId }).IsUnique();

            builder.HasOne(sr => sr.Player)
                .WithMany() // Player не е нужно да пази колекция от доклади в себе си
                .HasForeignKey(sr => sr.PlayerId)
                .OnDelete(DeleteBehavior.Restrict); // Предпазва от циклично изтриване

            builder.HasOne(sr => sr.Agency)
                .WithMany() // Agency също няма нужда от колекция от всички доклади
                .HasForeignKey(sr => sr.AgencyId)
                .OnDelete(DeleteBehavior.Cascade); // Ако агенцията фалира, докладите ѝ изчезват

            // Форматиране на валутите
            builder.Property(sr => sr.EstimatedMarketValue).HasPrecision(18, 4);
            builder.Property(sr => sr.EstimatedWageDemand).HasPrecision(18, 4);
        }
    }
}