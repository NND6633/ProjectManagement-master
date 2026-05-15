using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Models.Entities;
using ProjectManagement.Models.Identity;

namespace ProjectManagement.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserSkill> UserSkills { get; set; }

        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<TeamInvitation> TeamInvitations { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectDocument> ProjectDocuments { get; set; }

        public DbSet<MainTask> MainTasks { get; set; }
        public DbSet<Subtask> Subtasks { get; set; }
        public DbSet<PersonalTask> PersonalTasks { get; set; }

        public DbSet<Comment> Comments { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        public DbSet<AIResult> AIResults { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                var properties = entityType.ClrType.GetProperties()
                    .Where(p => p.PropertyType.IsEnum);

                foreach (var property in properties)
                {
                    builder.Entity(entityType.Name)
                        .Property(property.Name)
                        .HasConversion<string>();
                }
            }


            builder.Entity<Team>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Entity<Project>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Entity<ApplicationUser>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Entity<Comment>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Entity<Notification>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Entity<AIResult>()
                .Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");



            builder.Entity<MainTask>()
                .Property(x => x.Progress)
                .HasPrecision(5, 2); 

            builder.Entity<AIResult>()
                .Property(x => x.ConfidenceScore)
                .HasPrecision(3, 2);
        }
    }
}