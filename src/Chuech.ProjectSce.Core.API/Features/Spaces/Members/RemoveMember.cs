using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Data.Abstractions;
using Chuech.ProjectSce.Core.API.Features.Spaces.Authorization;
using Chuech.ProjectSce.Core.API.Infrastructure.DurableCommands;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members;
public static class RemoveMember
{
    public record UserCommand(int SpaceId, int UserId) : IRequest<Unit>;
    public record GroupCommand(int SpaceId, int GroupId) : IRequest<Unit>;
    public class GenericRemover
    {
        private readonly AuthBarrier<ISpaceAuthorizationService> _authBarrier;
        private readonly CoreContext _coreContext;
        private readonly IBus _bus;

        public GenericRemover(AuthBarrier<ISpaceAuthorizationService> authBarrier, CoreContext coreContext, IBus bus)
        {
            _authBarrier = authBarrier;
            _coreContext = coreContext;
            _bus = bus;
        }

        public async Task Remove(int spaceId, Func<SpaceMember, bool> memberPredicate, Guid requestId)
        {
            var initiatorId = _authBarrier.GetAuthorizedUserIdAsync(
                (auth, userId) => auth.AuthorizeAsync(spaceId, userId, SpacePermission.ManageMembers)
            );

            var space = await _coreContext.Spaces.LoadedForEdit().FirstAsync(x => x.Id == spaceId);
            var member = space.Members.FirstOrDefault(memberPredicate);
            if (member is null)
            {
                throw new NotFoundException("Member not found.");
            }

            space.RemoveMember(member.Id);
            _coreContext.LogOperation<GenericRemover>(requestId);

            try
            {
                await _coreContext.SaveChangesAsync();
            }
            catch (DuplicateOperationLogException)
            {
                // Idempotency; ignore.
            }

            await _bus.Publish(
                new SpaceMemberRemoved(space.Id,
                    (member as UserSpaceMember)?.UserId,
                    (member as GroupSpaceMember)?.GroupId)
           );
        }
    }
    public class UserHandler : DurableCommandHandler<UserCommand, Unit>
    {
        private readonly GenericRemover _genericRemover;

        public UserHandler(GenericRemover genericRemover,
            IBus bus, IRequestClient<ProcessDurableCommand<UserCommand>> requestClient) : base(bus, requestClient)
        {
            _genericRemover = genericRemover;
        }

        protected override async Task<Unit> HandleIdempotently(UserCommand command, Guid requestId)
        {
            await _genericRemover.Remove(command.SpaceId,
                x => x is UserSpaceMember userMember && userMember.UserId == command.UserId,
                requestId);

            return Unit.Value;
        }
    }
    public class GroupHandler : DurableCommandHandler<GroupCommand, Unit>
    {
        private readonly GenericRemover _genericRemover;

        public GroupHandler(GenericRemover genericRemover,
            IBus bus, IRequestClient<ProcessDurableCommand<GroupCommand>> requestClient) : base(bus, requestClient)
        {
            _genericRemover = genericRemover;
        }

        protected override async Task<Unit> HandleIdempotently(GroupCommand command, Guid requestId)
        {
            await _genericRemover.Remove(command.SpaceId,
                x => x is GroupSpaceMember groupMember && groupMember.GroupId == command.GroupId,
                requestId);

            return Unit.Value;
        }
    }
}
