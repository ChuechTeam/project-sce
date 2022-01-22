using Chuech.ProjectSce.Core.API.Data.Abstractions;
using Chuech.ProjectSce.Core.API.Features.Institutions;
using Chuech.ProjectSce.Core.API.Features.Users;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chuech.ProjectSce.Core.API.Features.Groups;

public sealed class Group : IFullyDatedEntity, ITransformPersistenceExceptions, ISuppressible
{
    public const string UniqueNameIndex = "ix_group_name";

    private Group()
    {
        Name = null!;
    }

    public Group(int institutionId, string name)
    {
        Name = name;
        InstitutionId = institutionId;
    }

    public int Id { get; private set; }
    public string Name { get; private set; }

    public Institution Institution { get; private set; } = null!;
    public int InstitutionId { get; private set; }

    public ICollection<User> Users { get; private set; } = new HashSet<User>();

    public Instant CreationDate { get; set; }
    public Instant LastEditDate { get; set; }

    public Instant? SuppressionDate { get; private set; }

    public void MarkAsSuppressed(IClock clock)
    {
        EnsureNotSuppressed();

        SuppressionDate = clock.GetCurrentInstant();
    }

    public void UpdateName(string newName)
    {
        EnsureNotSuppressed();

        Name = newName;
    }

    private void EnsureNotSuppressed()
    {
        if (SuppressionDate is not null)
        {
            throw new InvalidOperationException("Cannot do this operation on a suppressed group.");
        }
    }

    internal class Configuration : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            builder.HasIndex(x => new { x.Name, x.InstitutionId })
                .IsUnique()
                .HasDatabaseName(UniqueNameIndex);

            builder.HasMany(x => x.Users)
                .WithMany(x => x.Groups)
                .UsingEntity<GroupUser>(
                    j => j.HasOne(x => x.User)
                        .WithMany()
                        .HasForeignKey(x => x.UserId),
                    j => j.HasOne(x => x.Group)
                        .WithMany()
                        .HasForeignKey(x => x.GroupId),
                    j => j.HasKey(x => new { x.GroupId, x.UserId }).HasName(GroupUser.KeyName)
                );
        }
    }

    public void Rethrow(DbUpdateException exception)
    {
        if (exception is UniqueConstraintException && exception.Message.Contains(UniqueNameIndex))
        {
            throw Errors.NameTaken.AsException();
        }
    }

    public static class Errors
    {
        public static readonly Error UserCannotEnter =
            new("A user could not enter the group.", "group.userCannotEnter");

        public static readonly Error NameTaken =
            new("The name is already in use.", "group.nameTaken");

        public static readonly Error UserAlreadyPresent =
            new("A user is already in the group", "group.userAlreadyPresent");
    }
}