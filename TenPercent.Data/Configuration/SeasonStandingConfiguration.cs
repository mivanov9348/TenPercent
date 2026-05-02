namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models;

    public class SeasonStandingConfiguration : IEntityTypeConfiguration<SeasonStanding>
    {
        public void Configure(EntityTypeBuilder<SeasonStanding> builder)
        {
            builder.HasOne(ss => ss.Season)
                .WithMany(s => s.Standings)
                .HasForeignKey(ss => ss.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ss => ss.League)
                .WithMany()
                .HasForeignKey(ss => ss.LeagueId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ss => ss.Club)
                .WithMany()
                .HasForeignKey(ss => ss.ClubId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}