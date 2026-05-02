namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models;

    public class AgencyConfiguration : IEntityTypeConfiguration<Agency>
    {
        public void Configure(EntityTypeBuilder<Agency> builder)
        {
            builder.HasMany(a => a.Players)
                .WithOne(p => p.Agency)
                .HasForeignKey(p => p.AgencyId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(a => a.Budget)
                .HasPrecision(18, 2);
        }
    }
}