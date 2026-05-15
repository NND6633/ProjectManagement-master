using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Models.Entities;

namespace ProjectManagement.Data.Configurations
{
    public class SubtaskConfiguration : IEntityTypeConfiguration<Subtask>
    {
        public void Configure(EntityTypeBuilder<Subtask> builder)
        {
            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.HasOne(x => x.MainTask)
                .WithMany(m => m.Subtasks)
                .HasForeignKey(x => x.MainTaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Assignee)
                .WithMany()
                .HasForeignKey(x => x.AssigneeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Property(x => x.Status)
                .HasConversion<string>();

            builder.HasIndex(x => x.AssigneeId);
        }
    }
}
