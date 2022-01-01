using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.Members;
using Chuech.ProjectSce.Core.API.Features.Spaces.Members.Commands;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members;

public class RemoveSpaceMemberWhenInstitutionQuitConsumer : IConsumer<InstitutionMemberQuit>
{
    private readonly CoreContext _coreContext;
    private readonly ILogger<RemoveSpaceMemberWhenInstitutionQuitConsumer> _logger;

    public RemoveSpaceMemberWhenInstitutionQuitConsumer(CoreContext coreContext,
        ILogger<RemoveSpaceMemberWhenInstitutionQuitConsumer> logger)
    {
        _coreContext = coreContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<InstitutionMemberQuit> context)
    {
        var message = context.Message;

        var spaceIdsToClear = await _coreContext.Spaces.Where(s =>
                s.InstitutionId == message.InstitutionId &&
                s.Members.OfType<UserSpaceMember>()
                    .Any(x => x.UserId == message.UserId && x.CreationDate < message.OccurredTime))
            .Select(x => x.Id)
            .ToListAsync();

        if (spaceIdsToClear.Any())
        {
            _logger.LogInformation("Removing space members after institution quit in spaces {@Ids}", spaceIdsToClear);

            var events = spaceIdsToClear
                .Select(spaceId => new RemoveSpaceMember(spaceId, message.UserId, IsExceptional: true))
                .ToList();
            await context.PublishBatch(events);
        }
    }
}