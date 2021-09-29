
using Chuech.ProjectSce.Core.API.Data;
using System.Threading;

namespace Chuech.ProjectSce.Core.API.Features.Groups;
public class AccessibleUsersFromIdsQuery
{
    private readonly CoreContext _coreContext;

    public AccessibleUsersFromIdsQuery(CoreContext coreContext)
    {
        _coreContext = coreContext;
    }

    public async Task<User[]> Get(IEnumerable<int> userIds, int institutionId)
    {
        return await _coreContext.Users
                    .Where(x => userIds.Contains(x.Id) &&
                                x.InstitutionMembers.Any(x => x.InstitutionId == institutionId))
                    .ToArrayAsync();
    }
}
