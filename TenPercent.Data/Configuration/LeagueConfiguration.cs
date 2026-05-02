namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models;

    public class LeagueConfiguration : IEntityTypeConfiguration<League>
    {
        public void Configure(EntityTypeBuilder<League> builder)
        {
            builder.HasMany(l => l.Clubs)
                .WithOne(c => c.League)
                .HasForeignKey(c => c.LeagueId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}