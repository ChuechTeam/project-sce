using System;
using System.Threading;
using System.Threading.Tasks;
using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Data.Abstractions;
using Chuech.ProjectSce.Core.API.Features.Groups.Commands;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using MassTransit;
using MediatR;

namespace Chuech.ProjectSce.Core.API.Features.Groups;

public static class RemoveGroup
{
    public record Command([property: JsonIgnore] int GroupId) : IRequest<OperationResult>;

    public class Handler : IRequestHandler<Command, OperationResult>
    {
        private readonly CoreContext _coreContext;
        private readonly IRequestClient<SuppressGroup> _suppressClient;
        private readonly AuthBarrier<InstitutionAuthorizationService> _authBarrier;

        public Handler(CoreContext coreContext, IRequestClient<SuppressGroup> suppressClient,
            AuthBarrier<InstitutionAuthorizationService> authBarrier)
        {
            _coreContext = coreContext;
            _suppressClient = suppressClient;
            _authBarrier = authBarrier;
        }

        public async Task<OperationResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var group = await _coreContext.Groups
                .ExcludeSuppressed()
                .Select(x => new { x.Id, x.InstitutionId })
                .FirstOrDefaultAsync(x => x.Id == request.GroupId, cancellationToken);

            if (group is null)
            {
                return OperationResult.Failure(new Error(Kind: ErrorKind.NotFound));
            }

            _ = _authBarrier.GetAuthorizedUserIdAsync(
                (auth, userId) => auth.AuthorizeAsync(group.InstitutionId, userId, InstitutionPermission.ManageGroups));

            Response response = await _suppressClient.GetResponse(new SuppressGroup(group.Id), cancellationToken);
            return response switch
            {
                (_, SuppressGroup.Success) => OperationResult.Success(),
                (_, SuppressGroup.NotFound) => OperationResult.Failure(new Error(Kind: ErrorKind.NotFound)),
                _ => throw new InvalidOperationException("Unknown response type.")
            };
        }
    }
}