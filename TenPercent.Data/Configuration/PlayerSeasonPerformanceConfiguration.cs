namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models;

    public class PlayerSeasonPerformanceConfiguration : IEntityTypeConfiguration<PlayerSeasonPerformance>
    {
        public void Configure(EntityTypeBuilder<PlayerSeasonPerformance> builder)
        {
            builder.HasOne(pss => pss.Season)
                .WithMany(s => s.PlayerStats)
                .HasForeignKey(pss => pss.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pss => pss.Player)
                .WithMany()
                .HasForeignKey(pss => pss.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pss => pss.Club)
                .WithMany()
                .HasForeignKey(pss => pss.ClubId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(pss => pss.AverageRating)
                .HasPrecision(4, 1);
        }
    }
}