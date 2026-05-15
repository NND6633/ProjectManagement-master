using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Models.Entities;

namespace ProjectManagement.Data.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasOne(x => x.Team)
                .WithMany(t => t.Projects)
                .HasForeignKey(x => x.TeamId);

            builder.Property(x => x.Status)
                .HasConversion<string>();
        }
    }
}
