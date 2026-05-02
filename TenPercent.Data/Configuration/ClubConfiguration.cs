namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models;

    public class ClubConfiguration : IEntityTypeConfiguration<Club>
    {
        public void Configure(EntityTypeBuilder<Club> builder)
        {
            builder.HasMany(c => c.Players)
                .WithOne(p => p.Club)
                .HasForeignKey(p => p.ClubId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(c => c.TransferBudget)
                .HasPrecision(18, 2);

            builder.Property(c => c.WageBudget)
                .HasPrecision(18, 2);
        }
    }
}