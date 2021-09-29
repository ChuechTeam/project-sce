using Chuech.ProjectSce.Core.API.Features.Users;

namespace Chuech.ProjectSce.Core.API.Features.Resources.Authorization
{
    public class ResourceAuthorizationBehavior<TRequest, TResponse> : AuthorizationBehavior<TRequest, TResponse>
        where TRequest : IResourceRequest
    {
        private readonly IResourceAuthorizationService _resourceAuthorizationService;

        public ResourceAuthorizationBehavior(IAuthenticationService authenticationService,
            IResourceAuthorizationService resourceAuthorizationService,
            ILogger<ResourceAuthorizationBehavior<TRequest, TResponse>> logger) : base(authenticationService, logger)
        {
            _resourceAuthorizationService = resourceAuthorizationService;
        }

        protected override async ValueTask<AuthorizationResult> AuthorizeAsync(TRequest request, int userId,
            CancellationToken cancellationToken)
        {
            if (!AttributeCache<UseResourceAuthorizationAttribute>.Get(request.GetType()).Any())
            {
                return AuthorizationResult.Success;
            }

            return await _resourceAuthorizationService.AuthorizeAsync(request.ResourceId, userId);
        }
    }
}