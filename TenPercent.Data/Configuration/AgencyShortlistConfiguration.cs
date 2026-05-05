namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models;

    public class AgencyShortlistConfiguration : IEntityTypeConfiguration<AgencyShortlist>
    {
        public void Configure(EntityTypeBuilder<AgencyShortlist> builder)
        {
            // Композитен ключ
            builder.HasKey(s => new { s.AgencyId, s.PlayerId });

            builder.HasOne(s => s.Agency)
                .WithMany(a => a.Shortlist)
                .HasForeignKey(s => s.AgencyId)
                .OnDelete(DeleteBehavior.Cascade); // Ако изтриеш агенцията, триеш и нейния шортлист

            builder.HasOne(s => s.Player)
                .WithMany(p => p.ShortlistedBy)
                .HasForeignKey(s => s.PlayerId)
                .OnDelete(DeleteBehavior.Cascade); // Ако изтриеш играча, той изчезва от шортлистите
        }
    }
}