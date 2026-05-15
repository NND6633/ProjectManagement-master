using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Models.Entities;

namespace ProjectManagement.Data.Configurations
{
    public class CommentConfiguration : IEntityTypeConfiguration<Comment>
    {
        public void Configure(EntityTypeBuilder<Comment> builder)
        {
            builder.Property(x => x.Content)
                .IsRequired()
                .HasMaxLength(1000);

            builder.HasOne(x => x.Subtask)
                .WithMany(s => s.Comments)
                .HasForeignKey(x => x.SubtaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId);
        }
    }
}
