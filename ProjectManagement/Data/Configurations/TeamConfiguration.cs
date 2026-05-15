using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Models.Entities;

namespace ProjectManagement.Data.Configurations
{
    public class TeamConfiguration : IEntityTypeConfiguration<Team>
    {
        public void Configure(EntityTypeBuilder<Team> builder)
        {
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasOne(x => x.Leader)
                .WithMany()
                .HasForeignKey(x => x.LeaderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
