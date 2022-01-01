using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Users;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Resources.Documents.UploadSessions.Endpoint;

public static class ReportUploadDone
{
    public record Command(Guid SessionId) : IRequest<Unit>;

    public class Handler : IRequestHandler<Command, Unit>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly CoreContext _coreContext;
        private readonly IPublishEndpoint _publishEndpoint;

        public Handler(IAuthenticationService authenticationService, CoreContext coreContext,
            IPublishEndpoint publishEndpoint)
        {
            _authenticationService = authenticationService;
            _coreContext = coreContext;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = _authenticationService.GetUserId();
            if (!await _coreContext.DocumentUploadSessions.AnyAsync(x =>
                    x.CorrelationId == request.SessionId && x.AuthorId == userId, cancellationToken))
            {
                throw new NotFoundException();
            }

            await _publishEndpoint.Publish(new ClientDocumentFileUploadDoneReported(request.SessionId),
                cancellationToken);

            return Unit.Value;
        }
    }
}