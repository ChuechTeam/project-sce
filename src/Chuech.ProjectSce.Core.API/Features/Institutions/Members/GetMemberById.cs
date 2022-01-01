using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Institutions.Members.ApiModels;
using Chuech.ProjectSce.Core.API.Features.Users;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Members;

public static class GetMemberById
{
    public record Query(int InstitutionId, int UserId, bool IncludeAuthorizationInfo)
        : IRequest<InstitutionMemberApiModel?>, IInstitutionRequest;

    public class Handler : IRequestHandler<Query, InstitutionMemberApiModel?>
    {
        private readonly CoreContext _coreContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly InstitutionAuthorizationService _authorizationService;
        
        public Handler(CoreContext coreContext, 
            IAuthenticationService authenticationService,
            InstitutionAuthorizationService authorizationService)
        {
            _coreContext = coreContext;
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
        }

        public async Task<InstitutionMemberApiModel?> Handle(Query request, CancellationToken cancellationToken)
        {
            var initiatorId = _authenticationService.GetUserId();
            var authorizationThumbprint =
                await _authorizationService.GetThumbprintAsync(request.InstitutionId, initiatorId);
            
            _authorizationService.Authorize(authorizationThumbprint).ThrowIfFailed();

            var member = await _coreContext.InstitutionMembers
                .Where(x => x.InstitutionId == request.InstitutionId && x.UserId == request.UserId)
                .MapWith(InstitutionMemberApiModel.Mapper)
                .FirstOrDefaultAsync(cancellationToken);

            if (request.IncludeAuthorizationInfo && member?.UserId == initiatorId)
            {
                member.Permissions = authorizationThumbprint!.Permissions;
            }

            return member;
        }
    }
}