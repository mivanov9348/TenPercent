namespace TenPercent.Data.Configuration
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using TenPercent.Data.Models;

    public class MessageTemplateConfiguration : IEntityTypeConfiguration<MessageTemplate>
    {
        public void Configure(EntityTypeBuilder<MessageTemplate> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.SubjectTemplate)
                .IsRequired()
                .HasMaxLength(250);

            builder.Property(t => t.ContentTemplate)
                .IsRequired();
        }
    }
}