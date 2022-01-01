using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Data.Abstractions;
using MassTransit;
using NodaTime.Extensions;

namespace Chuech.ProjectSce.Core.API.Features.Groups.Commands;

public class RemoveGroupUserConsumer : IConsumer<RemoveGroupUser>
{
    private readonly CoreContext _coreContext;

    public RemoveGroupUserConsumer(CoreContext coreContext)
    {
        _coreContext = coreContext;
    }

    public async Task Consume(ConsumeContext<RemoveGroupUser> context)
    {
        var (groupId, userId) = context.Message;

        var data = await _coreContext.GroupUsers
            .Where(x => x.GroupId == groupId && x.UserId == userId)
            .Select(x => new
            {
                GroupUser = x,
                IsSuppressed = x.Group.SuppressionDate != null
            })
            .FirstOrDefaultAsync();

        var (groupUser, isSuppressed) = (data?.GroupUser, data?.IsSuppressed ?? false);

        if (isSuppressed)
        {
            await context.RespondIfNeededAsync(new Error("Group not found", Kind: ErrorKind.NotFound));
            return;
        }

        if (groupUser is not null)
        {
            // Make sure that we're not removing a user that joined past the time that command
            // was issued.
            if (!(context.SentTime is { } sentTime && groupUser.CreationDate > sentTime.ToInstant()))
            {
                _coreContext.GroupUsers.Remove(groupUser);
            }
        }

        await _coreContext.SaveChangesAsync();
        await context.Publish(new GroupUserQuit(groupId, userId));
        await context.RespondIfNeededAsync(new RemoveGroupUser.Success());
    }
}