using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Resources.Documents.UploadSessions.ApiModels;
using Chuech.ProjectSce.Core.API.Features.Users;

namespace Chuech.ProjectSce.Core.API.Features.Resources.Documents.UploadSessions.Endpoint;

public static class GetSessionById
{
    public record Query(Guid SessionId) : IRequest<DocumentUploadSessionApiModel?>;

    public class Handler : IRequestHandler<Query, DocumentUploadSessionApiModel?>
    {
        private readonly CoreContext _coreContext;
        private readonly IAuthenticationService _authenticationService;

        public Handler(CoreContext coreContext, IAuthenticationService authenticationService)
        {
            _coreContext = coreContext;
            _authenticationService = authenticationService;
        }

        public async Task<DocumentUploadSessionApiModel?> Handle(Query request, CancellationToken cancellationToken)
        {
            var userId = _authenticationService.GetUserId();
            return await _coreContext.DocumentUploadSessions
                .Where(x => x.CorrelationId == request.SessionId && x.AuthorId == userId)
                .MapWith(DocumentUploadSessionApiModel.Mapper)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        }
    }
}