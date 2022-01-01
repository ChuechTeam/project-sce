using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Groups.Commands;
using Chuech.ProjectSce.Core.API.Features.Institutions.Members;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Groups;

public class RemoveUserFromGroupsWhenInstitutionQuitConsumer : IConsumer<InstitutionMemberQuit>
{
    private readonly CoreContext _coreContext;

    public RemoveUserFromGroupsWhenInstitutionQuitConsumer(CoreContext coreContext)
    {
        _coreContext = coreContext;
    }

    public async Task Consume(ConsumeContext<InstitutionMemberQuit> context)
    {
        var (institutionId, userId, occurredTime) = context.Message;

        var usersToRemove = await _coreContext.GroupUsers
            .Where(x => x.Group.InstitutionId == institutionId && x.UserId == userId &&
                        x.CreationDate < occurredTime)
            .Select(x => new { x.GroupId, x.UserId })
            .ToListAsync();

        var requests = usersToRemove
            .Select(x => new RemoveGroupUser(x.GroupId, x.UserId))
            .ToArray();

        await context.PublishBatch(requests);
    }
}