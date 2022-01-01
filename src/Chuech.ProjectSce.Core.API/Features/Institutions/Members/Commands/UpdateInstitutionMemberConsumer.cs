using Chuech.ProjectSce.Core.API.Data;
using MassTransit;
using NodaTime.Extensions;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Members.Commands;

public class UpdateInstitutionMemberConsumer : IConsumer<UpdateInstitutionMember>
{
    private readonly CoreContext _coreContext;

    public UpdateInstitutionMemberConsumer(CoreContext coreContext)
    {
        _coreContext = coreContext;
    }

    public async Task Consume(ConsumeContext<UpdateInstitutionMember> context)
    {
        var message = context.Message;

        var member = await _coreContext.InstitutionMembers
            .Include(x => x.Institution)
            .FirstOrDefaultAsync(x => x.InstitutionId == message.InstitutionId && x.UserId == message.UserId);
        if (member is null)
        {
            if (context.RequestId is not null)
            {
                await context.RespondAsync(new UpdateInstitutionMember.Failure(new Error(Kind: ErrorKind.NotFound)));
            }

            return;
        }

        try
        {
            if (message.InstitutionRole is { } newInstitutionRole)
            {
                member.UpdateInstitutionRole(newInstitutionRole, member.Institution);
            }

            if (message.EducationalRole is { } newEducationalRole)
            {
                member.UpdateEducationalRole(newEducationalRole);
            }

            await _coreContext.SaveChangesAsync();
        }
        catch (ProjectSceException e)
        {
            if (context.RequestId is not null)
            {
                await context.RespondAsync(new UpdateInstitutionMember.Failure(e.Error));
            }

            return;
        }

        await context.Publish(new InstitutionMemberUpdated(member.InstitutionId, member.UserId,
            member.LastEditDate));

        if (context.RequestId is not null)
        {
            await context.RespondAsync(new UpdateInstitutionMember.Success());
        }
    }
}