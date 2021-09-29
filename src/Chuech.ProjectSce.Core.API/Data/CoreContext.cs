using Chuech.ProjectSce.Core.API.Data.Abstractions;
using Chuech.ProjectSce.Core.API.Data.Resources;
using EntityFramework.Exceptions.Common;
using EntityFramework.Exceptions.PostgreSQL;
using Npgsql;
using Npgsql.NameTranslation;

namespace Chuech.ProjectSce.Core.API.Data
{
    public class CoreContext : DbContext
    {
        static CoreContext()
        {
            var naming = new NpgsqlSnakeCaseNameTranslator();
            
            NpgsqlConnection.GlobalTypeMapper.MapEnum<ResourceType>(nameTranslator: naming);
            NpgsqlConnection.GlobalTypeMapper.MapEnum<InstitutionRole>(nameTranslator: naming);
            NpgsqlConnection.GlobalTypeMapper.MapEnum<EducationalRole>(nameTranslator: naming);
        }

        public CoreContext(DbContextOptions<CoreContext> options) : base(options)
        {
        }

        public DbSet<Institution> Institutions { get; private set; } = null!;
        public DbSet<InstitutionMember> InstitutionMembers { get; private set; } = null!;
        public DbSet<User> Users { get; private set; } = null!;
        public DbSet<Invitation> Invitations { get; private set; } = null!;
        public DbSet<Resource> Resources { get; private set; } = null!;
        public DbSet<DocumentResource> DocumentResources { get; private set; } = null!;
        public DbSet<Subject> Subjects { get; private set; } = null!;

        public DbSet<Space> Spaces { get; private set; } = null!;
        public DbSet<SpaceMember> SpaceMembers { get; private set; } = null!;

        public DbSet<Group> Groups { get; private set; } = null!;
        public DbSet<OperationLog> OperationLogs { get; private set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseExceptionProcessor()
                .UseSnakeCaseNamingConvention();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum<ResourceType>();
            modelBuilder.HasPostgresEnum<InstitutionRole>();
            modelBuilder.HasPostgresEnum<EducationalRole>();

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreContext).Assembly);
        }

        public void LogOperation(Guid id, string kind, object? result = null)
        {
            OperationLogs.Add(new OperationLog(id, kind, result));
        }

        public void LogOperation<T>(Guid id, object? result = null)
        {
            OperationLogs.Add(new OperationLog(id, typeof(T).FullName, result));
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<IHaveCreationDate>())
            {
                if (entry.State is EntityState.Added)
                {
                    entry.Entity.CreationDate = DateTime.UtcNow;
                }
            }

            foreach (var entry in ChangeTracker.Entries<IHaveLastEditDate>())
            {
                if (entry.State is EntityState.Added or EntityState.Modified)
                {
                    entry.Entity.LastEditDate = DateTime.UtcNow;
                }
            }

            try
            {
                return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
            } catch (UniqueConstraintException e) when (e.Message.Contains(OperationLog.UniqueIndexName))
            {
                throw new DuplicateOperationLogException("The operation has already been completed.", e);
            }
        }
    }
}