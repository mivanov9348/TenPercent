namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models;

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasOne(u => u.Agent)
                .WithOne(a => a.User)
                .HasForeignKey<Agent>(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}