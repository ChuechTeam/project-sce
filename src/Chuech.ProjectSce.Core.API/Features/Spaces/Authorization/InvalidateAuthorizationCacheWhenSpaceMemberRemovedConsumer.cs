using Chuech.ProjectSce.Core.API.Features.Spaces.Members;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Authorization;

public class InvalidateAuthorizationCacheWhenSpaceMemberRemovedConsumer : IConsumer<SpaceMemberRemoved>
{
    private readonly ISpaceUserAuthorizationInfoCache _authorizationInfoCache;

    public InvalidateAuthorizationCacheWhenSpaceMemberRemovedConsumer(ISpaceUserAuthorizationInfoCache authorizationInfoCache)
    {
        _authorizationInfoCache = authorizationInfoCache;
    }

    public async Task Consume(ConsumeContext<SpaceMemberRemoved> context)
    {
        var message = context.Message;
        if (message.UserId is { } removedUserId)
        {
            await _authorizationInfoCache.InvalidateAsync(message.SpaceId, removedUserId);
        } else
        {
            // We've removed a group, so we must invalidate all entries because
            // the group members can change between the removal and consumption time.

            await _authorizationInfoCache.InvalidateAllAsync(message.SpaceId);
        }
    }
}
