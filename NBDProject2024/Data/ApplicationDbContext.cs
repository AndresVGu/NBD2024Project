using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NBDProject2024.Models;

namespace NBDProject2024.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<RoleAuditLog> RoleAuditLogs { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RoleAuditLog>()
                .HasIndex(x => x.CreatedOnUtc);

            builder.Entity<RoleAuditLog>()
                .HasIndex(x => new { x.TargetUserId, x.CreatedOnUtc });
        }
    }

}
