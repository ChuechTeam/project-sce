using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Resources.Documents.UploadSessions.Endpoint;

public static class CreateSession
{
    public record Command(string Name, int InstitutionId, string FileExtension) : IRequest<Guid>;

    public class Handler : IRequestHandler<Command, Guid>
    {
        private readonly AuthBarrier<InstitutionAuthorizationService> _authBarrier;
        private readonly IRequestClient<StartDocumentUploadSession> _startClient;

        public Handler(AuthBarrier<InstitutionAuthorizationService> authBarrier,
            IRequestClient<StartDocumentUploadSession> startClient)
        {
            _authBarrier = authBarrier;
            _startClient = startClient;
        }

        public async Task<Guid> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = await _authBarrier.GetAuthorizedUserIdAsync(
                (auth, userId) => auth.AuthorizeAsync(request.InstitutionId, userId)
            );

            var sessionId = NewId.NextGuid();
            _ = await _startClient.GetResponse(new StartDocumentUploadSession(
                sessionId,
                request.Name,
                request.InstitutionId,
                userId,
                request.FileExtension
            ), cancellationToken);

            return sessionId;
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            Resource.ValidationRules.ValidateName(RuleFor(x => x.Name));
        }
    }
}