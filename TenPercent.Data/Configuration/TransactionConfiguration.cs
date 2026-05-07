namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models.Finance; 

    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.Property(t => t.Amount)
                .HasPrecision(18, 2); 

            // --- НОВО: Конфигурация за Season връзката ---
            builder.HasOne(t => t.Season)
                .WithMany()
                .HasForeignKey(t => t.SeasonId)
                .OnDelete(DeleteBehavior.Restrict); // Задължително Restrict!
        }
    }
}