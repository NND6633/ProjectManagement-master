using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Models.Entities;

namespace ProjectManagement.Data.Configurations
{
    public class UserSkillConfiguration : IEntityTypeConfiguration<UserSkill>
    {
        public void Configure(EntityTypeBuilder<UserSkill> builder)
        {
            builder.Property(x => x.SkillName)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasOne(x => x.User)
                .WithMany(u => u.Skills)
                .HasForeignKey(x => x.UserId);
        }
    }
}
