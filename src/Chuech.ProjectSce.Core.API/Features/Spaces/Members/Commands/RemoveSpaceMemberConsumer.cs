using Chuech.ProjectSce.Core.API.Data;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members.Commands;

public class RemoveSpaceMemberConsumer : IConsumer<RemoveSpaceMember>
{
    private readonly CoreContext _coreContext;

    public RemoveSpaceMemberConsumer(CoreContext coreContext)
    {
        _coreContext = coreContext;
    }

    public async Task Consume(ConsumeContext<RemoveSpaceMember> context)
    {
        var (spaceId, memberId, isExceptional) = context.Message;
        var messageId = context.MessageId ?? throw new InvalidOperationException("The MessageId is missing.");

        var removalTicket = await _coreContext.FindOperationLogAsync<RemoveSpaceMemberConsumer>(messageId);

        async Task Handle()
        {
            if (removalTicket is not null)
            {
                return;
            }

            var space = await _coreContext.Spaces.FirstOrDefaultAsync(x => x.Id == spaceId);
            var member = space?.Members.FirstOrDefault(x => x.Id == memberId);

            if (member is null)
            {
                throw new Error(Kind: ErrorKind.NotFound).AsException();
            }

            space!.RemoveMember(member.Id, allowExceptionalLackOfManagers: isExceptional);
            removalTicket = _coreContext.LogOperation<RemoveSpaceMemberConsumer>(messageId, new Output(member));

            try
            {
                await _coreContext.SaveChangesAsync();
            }
            catch (DuplicateOperationLogException)
            {
                // Already done.
            }
        }

        Task PublishEvents(Output output)
            => context.Publish(new SpaceMemberQuit(spaceId, output.UserId, output.GroupId));

        try
        {
            await Handle();
            await PublishEvents(removalTicket!.GetResultOrThrow<Output>());
            await context.RespondIfNeededAsync(new RemoveSpaceMember.Success());
        }
        catch (ProjectSceException e)
        {
            await context.RespondIfNeededAsync(new RemoveSpaceMember.Failure(e.Error));
        }
    }

    private record Output(int? UserId, int? GroupId)
    {
        public Output(SpaceMember member) : this((member as UserSpaceMember)?.UserId,
            (member as GroupSpaceMember)?.GroupId)
        {
        }
    }
}