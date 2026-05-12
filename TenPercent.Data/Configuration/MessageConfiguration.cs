namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models;

    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.HasKey(m => m.Id);

           
            builder.HasOne(m => m.ReceiverAgency)
                .WithMany() 
                .HasForeignKey(m => m.ReceiverAgencyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(m => m.SenderName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(m => m.Subject)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(m => m.Content)
                .IsRequired();
        }
    }
}