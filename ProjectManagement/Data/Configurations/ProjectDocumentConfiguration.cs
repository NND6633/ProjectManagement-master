using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Models.Entities;
namespace ProjectManagement.Data.Configurations
{
    public class ProjectDocumentConfiguration : IEntityTypeConfiguration<ProjectDocument>
    {
        public void Configure(EntityTypeBuilder<ProjectDocument> builder)
        {
            builder.Property(x => x.FileName)
                .IsRequired();

            builder.Property(x => x.FileUrl)
                .IsRequired();

            builder.HasOne(x => x.Project)
                .WithMany(p => p.Documents)
                .HasForeignKey(x => x.ProjectId);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UploadedBy)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
