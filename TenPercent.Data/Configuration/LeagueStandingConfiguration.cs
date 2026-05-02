namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models;

    public class LeagueStandingConfiguration : IEntityTypeConfiguration<LeagueStanding>
    {
        public void Configure(EntityTypeBuilder<LeagueStanding> builder)
        {
            builder.HasOne(ls => ls.League)
                .WithMany(l => l.LiveStandings)
                .HasForeignKey(ls => ls.LeagueId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ls => ls.Club)
                .WithOne(c => c.CurrentStanding)
                .HasForeignKey<LeagueStanding>(ls => ls.ClubId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}