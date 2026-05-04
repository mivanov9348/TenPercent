namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models;
    public class ClubContractConfiguration : IEntityTypeConfiguration<ClubContract>
    {
        public void Configure(EntityTypeBuilder<ClubContract> builder)
        {
            builder.Property(c => c.WeeklyWage).HasPrecision(18, 2);
            builder.Property(c => c.SigningBonus).HasPrecision(18, 2);
            builder.Property(c => c.AppearanceBonus).HasPrecision(18, 2);
            builder.Property(c => c.GoalBonus).HasPrecision(18, 2);
            builder.Property(c => c.CleanSheetBonus).HasPrecision(18, 2);
            builder.Property(c => c.ReleaseClause).HasPrecision(18, 2);

            builder.HasOne(c => c.Player)
                .WithMany(p => p.ClubContracts)
                .HasForeignKey(c => c.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Club)
                .WithMany() 
                .HasForeignKey(c => c.ClubId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}