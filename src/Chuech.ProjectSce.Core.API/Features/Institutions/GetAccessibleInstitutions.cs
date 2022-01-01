using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.ApiModels;
using Chuech.ProjectSce.Core.API.Features.Users;

namespace Chuech.ProjectSce.Core.API.Features.Institutions;

public static class GetAccessibleInstitutions
{
    public record Query : IRequest<IEnumerable<InstitutionApiModel>>;

    public class Handler : IRequestHandler<Query, IEnumerable<InstitutionApiModel>>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly CoreContext _coreContext;

        public Handler(CoreContext coreContext, IAuthenticationService authenticationService)
        {
            _coreContext = coreContext;
            _authenticationService = authenticationService;
        }

        public async Task<IEnumerable<InstitutionApiModel>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var userId = _authenticationService.GetUserId();

            var institutions = await _coreContext.Institutions
                .Where(x => x.Members.Any(y => y.UserId == userId))
                .MapWith(InstitutionApiModel.Mapper)
                .ToListAsync(cancellationToken);

            return institutions;
        }
    }
}