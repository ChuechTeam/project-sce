using Chuech.ProjectSce.Core.API.Features.Files.Storage;
using EntityFramework.Exceptions.PostgreSQL;

namespace Chuech.ProjectSce.Core.API.Features.Files.Data
{
    public sealed class FileStorageContext : DbContext
    {
        public FileStorageContext(DbContextOptions<FileStorageContext> options) : base(options)
        {
        }

        public DbSet<FileAccessLink> FileAccessLinks { get; private set; } = null!;

        public DbSet<TrackedUserFile> TrackedUserFiles { get; private set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseExceptionProcessor().UseSnakeCaseNamingConvention();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<FileAccessLink>(entity =>
            {
                entity.HasIndex(x => new { x.Id, x.ExpirationDate })
                    .IncludeProperties(x => x.FileName);
            });

            modelBuilder.Entity<TrackedUserFile>(entity =>
            {
                entity.Property(x => x.Category)
                    .HasConversion<string?>(
                        x => x == null ? null : x.Name,
                        x => x == null ? null : FileCategories.Find(x));

                // Related query:
                // Chuech.ProjectSce.Core.API.Features.Files.UserTrackingStorage.UserTrackingFileStorage.GetTotalUsedBytesAsync
                entity.HasIndex(x => x.UserId).IncludeProperties(x => x.FileSize);
                entity.HasIndex(x => new { x.UserId, x.FileName, x.Category });
            });
        }
    }
}