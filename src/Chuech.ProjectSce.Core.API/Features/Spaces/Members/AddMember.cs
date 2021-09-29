using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Data.Abstractions;
using Chuech.ProjectSce.Core.API.Features.Spaces.Authorization;
using Chuech.ProjectSce.Core.API.Infrastructure.DurableCommands;
using MassTransit;
using System.Text.Json.Serialization;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members;
public static class AddMember
{
    public record UserCommand([property: JsonIgnore] int SpaceId, int UserId, SpaceMemberCategory MemberCategory) : IRequest<Unit>;
    public record GroupCommand([property: JsonIgnore] int SpaceId, int GroupId, SpaceMemberCategory MemberCategory) : IRequest<Unit>;
    public abstract class CommonHandler<T> : DurableCommandHandler<T, Unit> where T : class, IRequest<Unit>
    {
        private readonly AuthBarrier<ISpaceAuthorizationService> _authBarrier;

        public CommonHandler(AuthBarrier<ISpaceAuthorizationService> authBarrier,
            IBus bus, IRequestClient<ProcessDurableCommand<T>> requestClient) : base(bus, requestClient)
        {
            _authBarrier = authBarrier;
        }

        protected Task<int> GetAuthorizedInitiatorId(int spaceId)
        {
            return _authBarrier.GetAuthorizedUserIdAsync(
                (auth, userId) => auth.AuthorizeAsync(spaceId, userId, SpacePermission.ManageMembers)
            );
        }
    }
    public class UserHandler : CommonHandler<UserCommand>
    {
        private readonly CoreContext _coreContext;

        public UserHandler(AuthBarrier<ISpaceAuthorizationService> authBarrier,
            IBus bus,
            IRequestClient<ProcessDurableCommand<UserCommand>> requestClient,
            CoreContext coreContext) : base(authBarrier, bus, requestClient)
        {
            _coreContext = coreContext;
        }

        protected override async Task<Unit> HandleIdempotently(UserCommand command, Guid requestId)
        {
            var (spaceId, memberId, memberCategory) = command;
            var initiatorId = await GetAuthorizedInitiatorId(spaceId);

            var space = await _coreContext.Spaces.LoadedForEdit().FirstAsync(x => x.Id == spaceId);

            if (!await _coreContext.InstitutionMembers
                .AnyAsync(x => x.UserId == memberId && x.InstitutionId == space.InstitutionId))
            {
                throw new NotFoundException("User not found.");
            }

            space.AddUserMember(memberId, memberCategory);
            _coreContext.LogOperation<UserHandler>(requestId);

            try
            {
                await _coreContext.SaveChangesAsync();
            }
            catch (DuplicateOperationLogException)
            {
                // Do nothing!
            }

            await Bus.Publish(new SpaceMemberAdded(spaceId, memberId, null));

            return Unit.Value;
        }
    }
    public class GroupHandler : CommonHandler<GroupCommand>
    {
        private readonly CoreContext _coreContext;

        public GroupHandler(AuthBarrier<ISpaceAuthorizationService> authBarrier,
            IBus bus,
            IRequestClient<ProcessDurableCommand<GroupCommand>> requestClient,
            CoreContext coreContext) : base(authBarrier, bus, requestClient)
        {
            _coreContext = coreContext;
        }

        protected override async Task<Unit> HandleIdempotently(GroupCommand command, Guid requestId)
        {
            var (spaceId, groupId, memberCategory) = command;
            var initiatorId = await GetAuthorizedInitiatorId(spaceId);

            var space = await _coreContext.Spaces.LoadedForEdit().FirstAsync(x => x.Id == spaceId);

            if (!await _coreContext.Groups.AnyAsync(x => x.Id == groupId && x.InstitutionId == space.InstitutionId))
            {
                throw new NotFoundException("Group not found.");
            }

            space.AddGroupMember(groupId, memberCategory);
            _coreContext.LogOperation<UserHandler>(requestId);
            try
            {
                await _coreContext.SaveChangesAsync();
            }
            catch (DuplicateOperationLogException)
            {
                // Do nothing!
            }

            await Bus.Publish(new SpaceMemberAdded(spaceId, null, groupId));

            return Unit.Value;
        }
    }
}
