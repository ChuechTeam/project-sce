using Chuech.ProjectSce.Core.API.Data.Abstractions;
using Chuech.ProjectSce.Core.API.Features.Institutions;
using Chuech.ProjectSce.Core.API.Features.Resources.Documents;
using Chuech.ProjectSce.Core.API.Features.Spaces;
using Chuech.ProjectSce.Core.API.Features.Users;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Chuech.ProjectSce.Core.API.Features.Resources;

public abstract class Resource : IFullyDatedEntity
{
    public Resource(ResourceType type)
    {
        Name = null!;
        Author = null!;
        Institution = null!;
        Type = type;
    }

    public Resource(string name, int institutionId, int authorId, ResourceType type, Guid id = default)
    {
        Id = id;
        Name = name;
        Author = null!;
        AuthorId = authorId;
        Institution = null!;
        InstitutionId = institutionId;
        Type = type;
    }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public Institution Institution { get; set; }
    public int InstitutionId { get; set; }

    public User Author { get; set; }
    public int AuthorId { get; set; }

    public Instant CreationDate { get; set; }
    public Instant LastEditDate { get; set; }

    public ResourceType Type { get; set; }

    private ICollection<ResourcePublication> _publicationLocations = new HashSet<ResourcePublication>();
    public IEnumerable<ResourcePublication> PublicationLocations => _publicationLocations;

    public ICollection<Space> PublishedSpaces { get; private set; } = null!;

    public async Task PublishAsync(IReadOnlyCollection<int> spaceIds, ResourcePublicationValidator validator)
    {
        if (!await validator.CanBePublishedInSpacesAsync(this, spaceIds))
        {
            throw new Error("Cannot publish this resource to one of the given spaces.",
                "resource.publicationImpossible").AsException();
        }

        foreach (var deletedLocation in _publicationLocations.Where(x => !spaceIds.Contains(x.SpaceId)))
        {
            _publicationLocations.Remove(deletedLocation);
        }
        foreach (var spaceId in spaceIds)
        {
            if (_publicationLocations.All(x => x.SpaceId != spaceId))
            {
                _publicationLocations.Add(new ResourcePublication(Id, spaceId));
            }
        }
    }

    internal class Configuration : IEntityTypeConfiguration<Resource>
    {
        public void Configure(EntityTypeBuilder<Resource> builder)
        {
            builder.HasDiscriminator(x => x.Type)
                .HasValue<DocumentResource>(ResourceType.Document);

            builder.HasMany(x => x.PublishedSpaces)
                .WithMany(x => x.PublishedResources)
                .UsingEntity<ResourcePublication>(
                    j => j.HasOne(x => x.Space)
                        .WithMany()
                        .HasForeignKey(x => x.SpaceId),
                    j => j.HasOne(x => x.Resource)
                        .WithMany(x => x.PublicationLocations)
                        .HasForeignKey(x => x.ResourceId),
                    j => j.HasKey(x => new { x.ResourceId, x.SpaceId }));

            builder.Navigation(x => x.PublicationLocations).AutoInclude();
        }
    }

    public static class Errors
    {
        public static readonly Error PublicationImpossible = new(
            "Cannot publish this resource to the given spaces.",
            "resource.publicationImpossible");

        public static readonly Error PermissionMissing = new(
            "You do not have permission to do this action.",
            "resource.permissionMissing");
    }

    public static class ValidationRules
    {
        public static IRuleBuilder<T, string> ValidateName<T>(IRuleBuilder<T, string> builder)
        {
            return builder
                .MaximumLength(100)
                .WithMessage("The resource name must not exceed the max length.")
                .WithErrorCode("resource.name.maxLengthExceeded");
        }
    }
}