using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Groups.Commands;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Groups;

public static class RemoveUser
{
    public record Command(int GroupId, int UserId) : IRequest<OperationResult>;

    public class Handler : IRequestHandler<Command, OperationResult>
    {
        private readonly CoreContext _coreContext;
        private readonly AuthBarrier<InstitutionAuthorizationService> _authBarrier;
        private readonly IRequestClient<RemoveGroupUser> _requestClient;

        public Handler(CoreContext coreContext,
            AuthBarrier<InstitutionAuthorizationService> authBarrier,
            IRequestClient<RemoveGroupUser> requestClient)
        {
            _coreContext = coreContext;
            _authBarrier = authBarrier;
            _requestClient = requestClient;
        }

        public async Task<OperationResult> Handle(Command request, CancellationToken cancellationToken)
        {
            if (await Authorize(request, cancellationToken) is { IsSuccess: false } failedAuth)
            {
                return failedAuth;
            }

            Response response = await _requestClient.GetResponse(new RemoveGroupUser(request.GroupId, request.UserId),
                cancellationToken);

            return response switch
            {
                (_, RemoveGroupUser.Success) => OperationResult.Success(),
                (_, RemoveGroupUser.Failure failure) => OperationResult.Failure(failure.Error),
                _ => throw new InvalidOperationException("Unknown response type.")
            };
        }

        private async Task<OperationResult> Authorize(Command request, CancellationToken cancellationToken)
        {
            var authInfo = await _coreContext.GroupUsers
                .Where(x => x.GroupId == request.GroupId && x.UserId == request.UserId &&
                            x.Group.SuppressionDate == null)
                .Select(x => new
                {
                    x.Group.InstitutionId
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (authInfo is null)
            {
                return OperationResult.Failure(new Error(Kind: ErrorKind.NotFound));
            }

            var authResult = await _authBarrier.GetAuthorizedUserIdResultAsync(
                (auth, userId) => auth.AuthorizeAsync(authInfo.InstitutionId, userId,
                    InstitutionPermission.ManageGroups));

            return authResult.WithoutResult();
        }
    }
}