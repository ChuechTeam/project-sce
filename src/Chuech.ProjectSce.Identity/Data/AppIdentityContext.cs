using System;
using Chuech.ProjectSce.Identity.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Chuech.ProjectSce.Identity.Data
{
    public class AppIdentityContext : IdentityDbContext<UserAccount>, IDisposable
    {
        public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

        public AppIdentityContext(DbContextOptions<AppIdentityContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserProfile>(entity =>
            {
                entity.Property(x => x.DisplayName)
                    .HasMaxLength(32);

                entity.HasIndex(x => x.DisplayName)
                    .IsUnique();
            });
        }
    }
}