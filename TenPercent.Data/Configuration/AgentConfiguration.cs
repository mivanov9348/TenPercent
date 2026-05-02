namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models;

    public class AgentConfiguration : IEntityTypeConfiguration<Agent>
    {
        public void Configure(EntityTypeBuilder<Agent> builder)
        {
            builder.HasOne(a => a.Agency)
                .WithOne(ag => ag.Agent)
                .HasForeignKey<Agency>(ag => ag.AgentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}