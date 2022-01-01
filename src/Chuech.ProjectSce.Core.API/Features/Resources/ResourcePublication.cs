using Chuech.ProjectSce.Core.API.Features.Spaces;

namespace Chuech.ProjectSce.Core.API.Features.Resources;

public class ResourcePublication
{
    public ResourcePublication(Guid resourceId, int spaceId)
    {
        ResourceId = resourceId;
        SpaceId = spaceId;
    }

    public Resource Resource { get; private set; } = null!;
    public Guid ResourceId { get; private set; }

    public Space Space { get; private set; } = null!;
    public int SpaceId { get; private set; }
}