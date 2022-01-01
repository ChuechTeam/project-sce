using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Spaces.Authorization;
using Chuech.ProjectSce.Core.API.Features.Spaces.Members.Commands;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members;

public static class RemoveMember
{
    public interface ICommonCommand : IRequest<OperationResult>
    {
        public int SpaceId { get; }
    }

    public record UserCommand(int SpaceId, int UserId) : ICommonCommand;

    public record GroupCommand(int SpaceId, int GroupId) : ICommonCommand;

    public abstract class CommonHandler<T> : IRequestHandler<T, OperationResult> where T : ICommonCommand
    {
        private readonly IRequestClient<RemoveSpaceMember> _requestClient;
        private readonly AuthBarrier<SpaceAuthorizationService> _authBarrier;
        protected CoreContext CoreContext { get; }

        public CommonHandler(IRequestClient<RemoveSpaceMember> requestClient,
            AuthBarrier<SpaceAuthorizationService> authBarrier, CoreContext coreContext)
        {
            _requestClient = requestClient;
            _authBarrier = authBarrier;
            CoreContext = coreContext;
        }

        public async Task<OperationResult> Handle(T request, CancellationToken cancellationToken)
        {
            _ = _authBarrier.GetAuthorizedUserIdAsync(
                (auth, userId) => auth.AuthorizeAsync(request.SpaceId, userId, SpacePermission.ManageMembers)
            );

            var memberId = await FindMemberId(request);
            if (memberId == default)
            {
                throw new NotFoundException();
            }

            Response response = await _requestClient.GetResponse(
                new RemoveSpaceMember(request.SpaceId, memberId, false), cancellationToken);

            return response is not (_, RemoveSpaceMember.Failure failure) ? 
                OperationResult.Success() : 
                OperationResult.Failure(failure.Error);
        }

        protected abstract Task<int> FindMemberId(T command);
    }

    public class GroupHandler : CommonHandler<GroupCommand>
    {
        public GroupHandler(IRequestClient<RemoveSpaceMember> requestClient,
            AuthBarrier<SpaceAuthorizationService> authBarrier, CoreContext coreContext) : base(requestClient,
            authBarrier, coreContext)
        {
        }

        protected override Task<int> FindMemberId(GroupCommand command)
        {
            return CoreContext.SpaceMembers.OfType<GroupSpaceMember>()
                .Where(x => x.Space.Id == command.SpaceId && x.GroupId == command.GroupId)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();
        }
    }

    public class UserHandler : CommonHandler<UserCommand>
    {
        public UserHandler(IRequestClient<RemoveSpaceMember> requestClient,
            AuthBarrier<SpaceAuthorizationService> authBarrier, CoreContext coreContext) : base(requestClient,
            authBarrier, coreContext)
        {
        }

        protected override Task<int> FindMemberId(UserCommand command)
        {
            return CoreContext.SpaceMembers.OfType<UserSpaceMember>()
                .Where(x => x.Space.Id == command.SpaceId && x.UserId == command.UserId)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();
        }
    }
}