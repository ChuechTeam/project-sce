using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Users;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Resources.Documents.UploadSessions.Endpoint;

public static class CancelSession
{
    public record Command(Guid SessionId) : IRequest<OperationResult>;

    public class Handler : IRequestHandler<Command, OperationResult>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly CoreContext _coreContext;
        private readonly IRequestClient<CancelDocumentUploadSession> _cancelClient;

        public Handler(CoreContext coreContext, IRequestClient<CancelDocumentUploadSession> cancelClient,
            IAuthenticationService authenticationService)
        {
            _coreContext = coreContext;
            _cancelClient = cancelClient;
            _authenticationService = authenticationService;
        }

        public async Task<OperationResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = _authenticationService.GetUserId();
            if (!await _coreContext.DocumentUploadSessions
                    .AnyAsync(x => x.CorrelationId == request.SessionId && x.AuthorId == userId, cancellationToken))
            {
                return OperationResult.Failure(new Error(Kind: ErrorKind.NotFound));
            }

            Response response =
                await _cancelClient.GetResponse(new CancelDocumentUploadSession(request.SessionId), cancellationToken);

            return response switch
            {
                (_, CancelDocumentUploadSession.Success) => OperationResult.Success(),
                (_, CancelDocumentUploadSession.Failure failure) => OperationResult.Failure(failure.Error),
                _ => throw new InvalidOperationException("Unknown response type.")
            };
        }
    }
}