using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.ApiModels;
using Chuech.ProjectSce.Core.API.Features.Users;

namespace Chuech.ProjectSce.Core.API.Features.Institutions;

public static class GetAccessibleInstitutionById
{
    public record Query(int SpaceId) : IRequest<InstitutionApiModel?>;
            
    public class Handler : IRequestHandler<Query, InstitutionApiModel?>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly CoreContext _coreContext;

        public Handler(CoreContext coreContext, IAuthenticationService authenticationService)
        {
            _coreContext = coreContext;
            _authenticationService = authenticationService;
        }

        public async Task<InstitutionApiModel?> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var userId = _authenticationService.GetUserId();

            var institution = await _coreContext.Institutions
                .Where(x => x.Members.Any(y => y.UserId == userId))
                .MapWith(InstitutionApiModel.Mapper)
                .FirstOrDefaultAsync(cancellationToken);

            return institution;
        }
    }
}