namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models;

    public class PlayerMatchPerformanceConfiguration : IEntityTypeConfiguration<PlayerMatchPerformance>
    {
        public void Configure(EntityTypeBuilder<PlayerMatchPerformance> builder)
        {
            builder.HasOne(pmp => pmp.Fixture)
                .WithMany(f => f.Performances)
                .HasForeignKey(pmp => pmp.FixtureId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pmp => pmp.Player)
                .WithMany()
                .HasForeignKey(pmp => pmp.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(pmp => pmp.MatchRating)
                .HasPrecision(4, 1);
        }
    }
}