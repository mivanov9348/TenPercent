namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models;

    public class PlayerConfiguration : IEntityTypeConfiguration<Player>
    {
        public void Configure(EntityTypeBuilder<Player> builder)
        {
            builder.Property(p => p.MarketValue)
                .HasPrecision(18, 2);

            builder.Property(p => p.WeeklyWage)
                .HasPrecision(18, 2);

            builder.HasOne(p => p.Attributes)
                .WithOne(pa => pa.Player)
                .HasForeignKey<PlayerAttributes>(pa => pa.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}