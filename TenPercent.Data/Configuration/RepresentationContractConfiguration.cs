namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models;

    public class RepresentationContractConfiguration : IEntityTypeConfiguration<RepresentationContract>
    {
        public void Configure(EntityTypeBuilder<RepresentationContract> builder)
        {
            builder.Property(c => c.IncomeCommissionPercentage).HasPrecision(5, 2);
            builder.Property(c => c.TransferCommissionPercentage).HasPrecision(5, 2);
            builder.Property(c => c.AgencyBrokerFee).HasPrecision(18, 2);
            builder.Property(c => c.SigningBonusPaid).HasPrecision(18, 2);
            builder.Property(c => c.AgencyReleaseClause).HasPrecision(18, 2);

            builder.HasOne(c => c.Player)
                .WithMany(p => p.RepresentationContracts)
                .HasForeignKey(c => c.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Agency)
                .WithMany()
                .HasForeignKey(c => c.AgencyId)
                .OnDelete(DeleteBehavior.Cascade);

           
        }
    }
}