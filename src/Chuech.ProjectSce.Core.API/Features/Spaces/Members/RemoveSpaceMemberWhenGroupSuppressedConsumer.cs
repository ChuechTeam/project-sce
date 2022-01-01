using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Groups;
using Chuech.ProjectSce.Core.API.Features.Spaces.Members.Commands;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members;

public class RemoveSpaceMemberWhenGroupSuppressedConsumer : IConsumer<GroupSuppressed>
{
    private readonly CoreContext _coreContext;
    private readonly ILogger<RemoveSpaceMemberWhenGroupSuppressedConsumer> _logger;

    public RemoveSpaceMemberWhenGroupSuppressedConsumer(CoreContext coreContext,
        ILogger<RemoveSpaceMemberWhenGroupSuppressedConsumer> logger)
    {
        _coreContext = coreContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<GroupSuppressed> context)
    {
        var groupId = context.Message.GroupId;

        var spaceMembers = await _coreContext.SpaceMembers
            .OfType<GroupSpaceMember>()
            .Where(x => x.Group.Id == groupId)
            .Select(x => new { x.SpaceId, x.Id })
            .ToArrayAsync();

        if (spaceMembers.Any())
        {
            _logger.LogInformation("Removing space members after group suppressed: {@Members}",
                new object[] { spaceMembers });

            var messages = spaceMembers
                .Select(x => new RemoveSpaceMember(x.SpaceId, x.Id, false))
                .ToArray();
            await context.PublishBatch(messages);
        }
    }
}