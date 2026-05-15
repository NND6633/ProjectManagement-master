using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Models.Entities;

namespace ProjectManagement.Data.Configurations
{
    public class PersonalTaskConfiguration : IEntityTypeConfiguration<PersonalTask>
    {
        public void Configure(EntityTypeBuilder<PersonalTask> builder)
        {
            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId);

            builder.Property(x => x.Status)
                .HasConversion<string>();
        }
    }
}
