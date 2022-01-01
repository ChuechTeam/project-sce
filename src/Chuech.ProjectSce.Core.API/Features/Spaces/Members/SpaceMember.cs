using Chuech.ProjectSce.Core.API.Data.Abstractions;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members;

public abstract class SpaceMember : IHaveCreationDate
{
    protected SpaceMember()
    {
        Space = null!;
    }

    protected SpaceMember(Space space, SpaceMemberCategory category)
    {
        Space = space;
        SpaceId = space.Id;
        Category = category;
    }

    public int Id { get; set; }

    public Space Space { get; set; }
    public int SpaceId { get; set; }

    public SpaceMemberCategory Category { get; set; }
    
    public Instant CreationDate { get; set; }
}