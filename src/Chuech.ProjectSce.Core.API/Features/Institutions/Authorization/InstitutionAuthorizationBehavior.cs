using Chuech.ProjectSce.Core.API.Features.Users;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Authorization
{
    public class InstitutionAuthorizationBehavior<TRequest, TResponse> : AuthorizationBehavior<TRequest, TResponse>
        where TRequest : IInstitutionRequest
    {
        private readonly IInstitutionAuthorizationService _institutionAuthorizationService;

        public InstitutionAuthorizationBehavior(
            ILogger<InstitutionAuthorizationBehavior<TRequest, TResponse>> logger,
            IAuthenticationService authenticationService,
            IInstitutionAuthorizationService institutionAuthorizationService) : base(authenticationService, logger)
        {
            _institutionAuthorizationService = institutionAuthorizationService;
        }
        
        protected override async ValueTask<AuthorizationResult> AuthorizeAsync(TRequest request, int userId,
            CancellationToken cancellationToken)
        {
            var attribute = AttributeCache<UseInstitutionAuthorizationAttribute>.Get(request.GetType()).SingleOrDefault();
            if (attribute is null)
            {
                return AuthorizationResult.Success;
            }

            var institutionId = request.InstitutionId;
            return await _institutionAuthorizationService.AuthorizeAsync(institutionId, userId, attribute.PermissionsRequired);
        }
    }
}