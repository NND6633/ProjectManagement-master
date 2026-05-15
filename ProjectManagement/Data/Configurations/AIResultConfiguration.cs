using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ProjectManagement.Models.Entities;

namespace ProjectManagement.Data.Configurations
{
    public class AIResultConfiguration : IEntityTypeConfiguration<AIResult>
    {
        public void Configure(EntityTypeBuilder<AIResult> builder)
        {
            builder.Property(x => x.InputJson)
                .IsRequired();

            builder.Property(x => x.OutputJson)
                .IsRequired();

            builder.Property(x => x.Type)
                .HasConversion<string>();
        }
    }
}
