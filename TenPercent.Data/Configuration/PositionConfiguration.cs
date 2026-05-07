namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models;

    public class PositionConfiguration : IEntityTypeConfiguration<Position>
    {
        public void Configure(EntityTypeBuilder<Position> builder)
        {
            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(p => p.Abbreviation)
                .IsRequired()
                .HasMaxLength(5);

            builder.Property(p => p.PaceWeight).HasPrecision(3, 2);
            builder.Property(p => p.ShootingWeight).HasPrecision(3, 2);
            builder.Property(p => p.PassingWeight).HasPrecision(3, 2);
            builder.Property(p => p.DribblingWeight).HasPrecision(3, 2);
            builder.Property(p => p.DefendingWeight).HasPrecision(3, 2);
            builder.Property(p => p.PhysicalWeight).HasPrecision(3, 2);
            builder.Property(p => p.GoalkeepingWeight).HasPrecision(3, 2);
            builder.Property(p => p.VisionWeight).HasPrecision(3, 2);
            builder.Property(p => p.StaminaWeight).HasPrecision(3, 2);
        }
    }
}