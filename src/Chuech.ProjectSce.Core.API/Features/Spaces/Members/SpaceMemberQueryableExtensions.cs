using Chuech.ProjectSce.Core.API.Data;
using System.Runtime.CompilerServices;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members;
public static class SpaceMemberQueryableExtensions
{
    public static IQueryable<SpaceMember> WhereRelatedToUser(this IQueryable<SpaceMember> spaceMembers, int userId)
    {
        return spaceMembers.Where(x =>
            x is UserSpaceMember ?
                ((UserSpaceMember)x).UserId == userId :
            x is GroupSpaceMember ?
                ((GroupSpaceMember)x).Group.Users.Any(x => x.Id == userId) : 
            false);
    }
}
