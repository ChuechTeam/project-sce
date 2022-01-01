namespace Chuech.ProjectSce.Core.API.Features.Resources.Commands;

public record PublishResource(Guid ResourceId, IReadOnlyCollection<int> SpaceIds)
    : IRespondWith<PublishResource.Success, PublishResource.Failure>
{
    public record Success;

    public record Failure(Error Error) : ProjectSceFailure(Error);
}