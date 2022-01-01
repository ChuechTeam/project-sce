namespace Chuech.ProjectSce.Core.API.Features.Resources;

public record ResourcePublished(Guid ResourceId, IReadOnlyCollection<int> SpaceIds);