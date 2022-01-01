using Chuech.ProjectSce.Core.API.Data;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Members.Commands;

public class RemoveInstitutionMemberConsumer : IConsumer<RemoveInstitutionMember>
{
    private readonly CoreContext _coreContext;
    private readonly InstitutionGatewayService _institutionGatewayService;

    public RemoveInstitutionMemberConsumer(CoreContext coreContext,
        InstitutionGatewayService institutionGatewayService)
    {
        _coreContext = coreContext;
        _institutionGatewayService = institutionGatewayService;
    }

    public async Task Consume(ConsumeContext<RemoveInstitutionMember> context)
    {
        var messageId = context.MessageId ?? throw new InvalidOperationException("No MessageId.");
        var (institutionId, userId) = context.Message;
        var removalTicket =
            await _coreContext.FindOperationLogAsync<RemoveInstitutionMemberConsumer>(messageId);

        async Task Handle()
        {
            var member = await _coreContext.InstitutionMembers.FindByPairAsync(userId, institutionId);
            if (member is null)
            {
                throw new NotFoundException();
            }

            await _institutionGatewayService.QuitAsync(member);
            removalTicket = _coreContext.LogOperation<RemoveInstitutionMemberConsumer>(messageId);

            await _coreContext.SaveChangesAsync();
        }

        async Task Publish()
        {
            await context.Publish(new InstitutionMemberQuit(institutionId, userId, removalTicket!.CreationDate));
        }
        
        if (removalTicket is null)
        {
            try
            {
                await Handle();
            }
            catch (ProjectSceException e)
            {
                if (context.RequestId is not null)
                {
                    await context.RespondAsync(new RemoveInstitutionMember.Failure(e.Error));
                }

                return;
            }
        }

        await Publish();
    }
}