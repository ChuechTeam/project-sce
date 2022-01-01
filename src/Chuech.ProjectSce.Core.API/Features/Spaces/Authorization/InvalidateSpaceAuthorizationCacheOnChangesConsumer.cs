using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Spaces.Members;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Authorization;

public class InvalidateSpaceAuthorizationCacheOnChangesConsumer
    : IConsumer<SpaceMemberQuit>, IConsumer<InstitutionAuthorizationChanged>
{
    private readonly ISpaceAuthorizationCache _authorizationCache;
    private readonly ILogger<InvalidateSpaceAuthorizationCacheOnChangesConsumer> _logger;
    private readonly CoreContext _coreContext;

    public InvalidateSpaceAuthorizationCacheOnChangesConsumer(
        ISpaceAuthorizationCache authorizationCache,
        ILogger<InvalidateSpaceAuthorizationCacheOnChangesConsumer> logger, CoreContext coreContext)
    {
        _authorizationCache = authorizationCache;
        _logger = logger;
        _coreContext = coreContext;
    }

    public async Task Consume(ConsumeContext<SpaceMemberQuit> context)
    {
        // We don't really care if the message has been sent multiple times, invalidating the
        // cache twice isn't a big deal.

        var (spaceId, userId, groupId) = context.Message;
        if (userId is { } removedUserId)
        {
            _logger.LogDebug("Invalidating space authorization cache for user {UserId} in space {SpaceId} " +
                             "as the user has quit the space", userId, spaceId);

            await _authorizationCache.InvalidateAsync(spaceId, removedUserId);
        }
        else
        {
            // We've removed a group, so we must invalidate all entries because
            // the group members can change between the removal and consumption time.

            _logger.LogDebug("Invalidating space authorization cache for all users in space {SpaceId} " +
                             "as the group {GroupId} has quit the space", spaceId, groupId);

            await _authorizationCache.InvalidateAllAsync(spaceId);
        }
    }

    public async Task Consume(ConsumeContext<InstitutionAuthorizationChanged> context)
    {
        var (institutionId, userIds) = context.Message;

        var spaceIds = await _coreContext.Spaces
            .Where(x => x.InstitutionId == institutionId)
            .Select(x => x.Id)
            .ToArrayAsync();

        _logger.LogDebug(
            "Invalidating space authorization cache for users {@UserIds} in spaces {@SpaceIds} " +
            "as the user's institution permissions changed",
            userIds, spaceIds);

        await Task.WhenAll(spaceIds.Select(spaceId => _authorizationCache.InvalidateAsync(spaceId, userIds.ToArray())));
    }
}