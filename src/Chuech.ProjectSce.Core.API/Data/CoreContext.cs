using Chuech.ProjectSce.Core.API.Data.Abstractions;
using Chuech.ProjectSce.Core.API.Features.Groups;
using Chuech.ProjectSce.Core.API.Features.Institutions;
using Chuech.ProjectSce.Core.API.Features.Institutions.Members;
using Chuech.ProjectSce.Core.API.Features.Invitations;
using Chuech.ProjectSce.Core.API.Features.Resources;
using Chuech.ProjectSce.Core.API.Features.Resources.Documents;
using Chuech.ProjectSce.Core.API.Features.Resources.Documents.UploadSessions;
using Chuech.ProjectSce.Core.API.Features.Spaces;
using Chuech.ProjectSce.Core.API.Features.Spaces.Members;
using Chuech.ProjectSce.Core.API.Features.Subjects;
using Chuech.ProjectSce.Core.API.Features.Users;
using EntityFramework.Exceptions.PostgreSQL;
using MassTransit.EntityFrameworkCoreIntegration;
using MassTransit.EntityFrameworkCoreIntegration.Mappings;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using Npgsql.NameTranslation;

namespace Chuech.ProjectSce.Core.API.Data;

public class CoreContext : SagaDbContext
{
    private readonly IClock _clock;

    static CoreContext()
    {
        var naming = new NpgsqlSnakeCaseNameTranslator();

        NpgsqlConnection.GlobalTypeMapper.MapEnum<ResourceType>(nameTranslator: naming);
        NpgsqlConnection.GlobalTypeMapper.MapEnum<InstitutionRole>(nameTranslator: naming);
        NpgsqlConnection.GlobalTypeMapper.MapEnum<EducationalRole>(nameTranslator: naming);
    }

    public static void ConfigureOptions(DbContextOptionsBuilder builder, string? connectionString)
    {
        builder.UseExceptionProcessor()
            .UseSnakeCaseNamingConvention();

        void ConfigureNpgsql(NpgsqlDbContextOptionsBuilder p)
        {
            p.UseNodaTime().UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);
        }

        if (connectionString is not null)
        {
            builder.UseNpgsql(connectionString, ConfigureNpgsql);
        }
        else
        {
            builder.UseNpgsql(ConfigureNpgsql);
        }
    }

    public CoreContext(DbContextOptions<CoreContext> options) : base(
        new DbContextOptionsBuilder<CoreContext>(options)
            .UseExceptionProcessor()
            .UseSnakeCaseNamingConvention()
            .Options)
    {
        _clock = this.GetService<IClock>();
    }

    public DbSet<Institution> Institutions { get; private set; } = null!;
    public DbSet<InstitutionMember> InstitutionMembers { get; private set; } = null!;

    public DbSet<User> Users { get; private set; } = null!;
    public DbSet<Invitation> Invitations { get; private set; } = null!;

    public DbSet<Resource> Resources { get; private set; } = null!;
    public DbSet<DocumentResource> DocumentResources { get; private set; } = null!;
    public DbSet<ResourcePublication> ResourcePublications { get; private set; } = null!;

    public DbSet<DocumentUploadSession> DocumentUploadSessions { get; private set; } = null!;

    public DbSet<Subject> Subjects { get; private set; } = null!;

    public DbSet<Space> Spaces { get; private set; } = null!;
    public DbSet<SpaceMember> SpaceMembers { get; private set; } = null!;

    public DbSet<Group> Groups { get; private set; } = null!;
    public DbSet<GroupUser> GroupUsers { get; private set; } = null!;

    public DbSet<OperationLog> OperationLogs { get; private set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresEnum<ResourceType>();
        modelBuilder.HasPostgresEnum<InstitutionRole>();
        modelBuilder.HasPostgresEnum<EducationalRole>();

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CoreContext).Assembly);
    }

    protected override IEnumerable<ISagaClassMap> Configurations => new ISagaClassMap[]
    {
        new DocumentUploadSession.Map()
    };

    public OperationLog LogOperation(Guid id, string kind, object? result = null)
    {
        return OperationLogs.Add(new OperationLog(id, kind, result)).Entity;
    }

    public OperationLog LogOperation<T>(Guid id, object? result = null)
    {
        return LogOperation(id, typeof(T).FullName ?? typeof(T).Name, result);
    }

    public Task<OperationLog?> FindOperationLogAsync<T>(Guid id)
    {
        return FindOperationLogAsync(id, typeof(T).FullName ?? typeof(T).Name);
    }

    public Task<OperationLog?> FindOperationLogAsync(Guid id, string kind)
    {
        return OperationLogs.FirstOrDefaultAsync(x => x.Id == id && x.Kind == kind);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IHaveCreationDate>())
        {
            if (entry.State is EntityState.Added)
            {
                entry.Entity.CreationDate = _clock.GetCurrentInstant();
            }
        }

        foreach (var entry in ChangeTracker.Entries<IHaveLastEditDate>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                entry.Entity.LastEditDate = _clock.GetCurrentInstant();
            }
        }

        try
        {
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException e)
        {
            ITransformPersistenceExceptions.Dispatch(e);
            throw;
        }
    }

    public class DesignTimeFactory : IDesignTimeDbContextFactory<CoreContext>
    {
        public CoreContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<CoreContext>();
            ConfigureOptions(builder, null);

            builder.UseApplicationServiceProvider(new ServiceCollection()
                .AddSingleton<IClock>(SystemClock.Instance)
                .BuildServiceProvider());

            return new CoreContext(builder.Options);
        }
    }
}