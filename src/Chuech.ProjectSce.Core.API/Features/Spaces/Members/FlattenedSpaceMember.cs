using Chuech.ProjectSce.Core.API.Data;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members;

public record FlattenedSpaceMember(int SpaceId, int[] UserIds, SpaceMemberCategory Category);
public static class FlattenedSpaceMemberQueryableExtensions
{
    public static IQueryable<FlattenedSpaceMember> Flattened(this IQueryable<SpaceMember> spaceMembers)
    {
        return spaceMembers.Select(x => new FlattenedSpaceMember
        (
            x.SpaceId,
            x is UserSpaceMember ?
                new int[] { ((UserSpaceMember)x).UserId } :
            x is GroupSpaceMember ?
                ((GroupSpaceMember)x).Group.Users.Select(x => x.Id).ToArray() : null!,
            x.Category
        ));
    }
}