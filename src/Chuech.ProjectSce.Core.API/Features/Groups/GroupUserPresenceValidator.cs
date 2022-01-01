using System.Collections.Immutable;
using Chuech.ProjectSce.Core.API.Data;

namespace Chuech.ProjectSce.Core.API.Features.Groups;

/// <summary>
/// Validates the presence of a group user, i.e. whether or not it is possible for that user to be in the group.
/// </summary>
public class GroupUserPresenceValidator
{
    private readonly CoreContext _coreContext;

    public GroupUserPresenceValidator(CoreContext coreContext)
    {
        _coreContext = coreContext;
    }

    public async Task<bool> MayBePresent(int userId, int institutionId)
    {
        return (await MayBePresent(new[] { userId }, institutionId))[userId];
    }

    public async Task<ImmutableDictionary<int, bool>> MayBePresent(IEnumerable<int> users, int institutionId)
    {
        var userIdArray = users.ToArray();
        var validUserIds = await _coreContext.InstitutionMembers
            .Where(x => x.InstitutionId == institutionId && userIdArray.Contains(x.UserId))
            .Select(x => x.UserId)
            .ToListAsync();

        return userIdArray.ToImmutableDictionary(k => k, x => validUserIds.Contains(x));
    }
}