using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Data.Abstractions;
using Chuech.ProjectSce.Core.API.Features.Spaces.Authorization;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members;

public static class AddMember
{
    public record UserCommand
        ([property: JsonIgnore] int SpaceId, int UserId, SpaceMemberCategory MemberCategory) : IRequest<Unit>;

    public record GroupCommand
        ([property: JsonIgnore] int SpaceId, int GroupId, SpaceMemberCategory MemberCategory) : IRequest<Unit>;

    public abstract class CommonHandler<T> : IRequestHandler<T> where T : class, IRequest<Unit>
    {
        private readonly AuthBarrier<SpaceAuthorizationService> _authBarrier;

        public CommonHandler(AuthBarrier<SpaceAuthorizationService> authBarrier)
        {
            _authBarrier = authBarrier;
        }

        protected Task<int> GetAuthorizedInitiatorId(int spaceId)
        {
            return _authBarrier.GetAuthorizedUserIdAsync(
                (auth, userId) => auth.AuthorizeAsync(spaceId, userId, SpacePermission.ManageMembers)
            );
        }

        public abstract Task<Unit> Handle(T request, CancellationToken cancellationToken);
    }

    public class UserHandler : CommonHandler<UserCommand>
    {
        private readonly CoreContext _coreContext;

        public UserHandler(AuthBarrier<SpaceAuthorizationService> authBarrier, CoreContext coreContext) : base(
            authBarrier)
        {
            _coreContext = coreContext;
        }

        public override async Task<Unit> Handle(UserCommand command, CancellationToken cancellationToken)
        {
            _ = await GetAuthorizedInitiatorId(command.SpaceId);

            var space = await _coreContext.Spaces.FirstAsync(x => x.Id == command.SpaceId, cancellationToken);

            if (!await _coreContext.InstitutionMembers
                    .AnyAsync(x => x.UserId == command.UserId && x.InstitutionId == space.InstitutionId,
                        cancellationToken))
            {
                throw new NotFoundException("User not found.");
            }

            space.AddUserMember(command.UserId, command.MemberCategory);
            await _coreContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }

    public class GroupHandler : CommonHandler<GroupCommand>
    {
        private readonly CoreContext _coreContext;

        public GroupHandler(AuthBarrier<SpaceAuthorizationService> authBarrier, CoreContext coreContext) : base(
            authBarrier)
        {
            _coreContext = coreContext;
        }

        public override async Task<Unit> Handle(GroupCommand command, CancellationToken cancellationToken)
        {
            _ = await GetAuthorizedInitiatorId(command.SpaceId);

            var space = await _coreContext.Spaces.FirstAsync(x => x.Id == command.SpaceId, cancellationToken);

            if (!await _coreContext.Groups
                    .ExcludeSuppressed()
                    .AnyAsync(x => x.Id == command.GroupId && x.InstitutionId == space.InstitutionId,
                        cancellationToken))
            {
                throw new NotFoundException("Group not found.");
            }

            space.AddGroupMember(command.GroupId, command.MemberCategory);
            await _coreContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}