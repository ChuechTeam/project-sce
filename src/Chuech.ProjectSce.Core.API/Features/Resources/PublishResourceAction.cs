using Chuech.ProjectSce.Core.API.Features.Resources.Authorization;
using Chuech.ProjectSce.Core.API.Features.Resources.Commands;
using Chuech.ProjectSce.Core.API.Features.Users;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Resources;

public static class PublishResourceAction
{
    public record Command([property: JsonIgnore] Guid ResourceId, int[] SpaceIds) : IRequest<OperationResult>;

    public class Handler : IRequestHandler<Command, OperationResult>
    {
        private readonly IRequestClient<PublishResource> _publishClient;
        private readonly ResourcePublicationAuthorizationService _publicationAuthorizationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<Handler> _logger;

        public Handler(IRequestClient<PublishResource> publishClient,
            ResourcePublicationAuthorizationService publicationAuthorizationService,
            IAuthenticationService authenticationService, ILogger<Handler> logger)
        {
            _publishClient = publishClient;
            _publicationAuthorizationService = publicationAuthorizationService;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        public async Task<OperationResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var (resourceId, spaceIds) = request;
            var userId = _authenticationService.GetUserId();

            var authResult = await _publicationAuthorizationService.AuthorizeAsync(resourceId, spaceIds, userId);
            if (authResult.Failed(out var authError))
            {
                return OperationResult.Failure(authError);
            }

            Response response =
                await _publishClient.GetResponse(new PublishResource(resourceId, spaceIds), cancellationToken);

            return response switch
            {
                (_, PublishResource.Failure failure) => OperationResult.Failure(failure.Error),
                (_, PublishResource.Success) => OperationResult.Success(),
                _ => throw new InvalidOperationException("Unknown response type.")
            };
        }
    }
}