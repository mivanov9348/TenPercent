namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models;

    public class FixtureConfiguration : IEntityTypeConfiguration<Fixture>
    {
        public void Configure(EntityTypeBuilder<Fixture> builder)
        {
            builder.HasOne(f => f.Season)
                .WithMany(s => s.Fixtures)
                .HasForeignKey(f => f.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.HomeClub)
                .WithMany()
                .HasForeignKey(f => f.HomeClubId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.AwayClub)
                .WithMany()
                .HasForeignKey(f => f.AwayClubId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}