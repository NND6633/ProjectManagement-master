using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Models.Entities;

namespace ProjectManagement.Data.Configurations
{
    public class MainTaskConfiguration : IEntityTypeConfiguration<MainTask>
    {
        public void Configure(EntityTypeBuilder<MainTask> builder)
        {
            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.HasOne(x => x.Project)
                .WithMany(p => p.MainTasks)
                .HasForeignKey(x => x.ProjectId);

            builder.HasOne(x => x.Creator)
                .WithMany()
                .HasForeignKey(x => x.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Property(x => x.Status)
                .HasConversion<string>();

            builder.HasIndex(x => x.ProjectId);
        }
    }
}
