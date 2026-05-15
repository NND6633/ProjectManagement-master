using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Models.Entities;

namespace ProjectManagement.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(500);

            builder.HasOne(x => x.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(x => x.UserId);

            builder.Property(x => x.Type)
                .HasConversion<string>();
        }
    }
}
