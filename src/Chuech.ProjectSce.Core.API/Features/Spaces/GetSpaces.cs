using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Spaces.ApiModels;
using Chuech.ProjectSce.Core.API.Features.Spaces.Authorization;
using Chuech.ProjectSce.Core.API.Features.Spaces.Members;
using Chuech.ProjectSce.Core.API.Features.Users;

namespace Chuech.ProjectSce.Core.API.Features.Spaces;

public static class GetSpaces
{
    public record Query(int InstitutionId) : IRequest<IEnumerable<SpaceApiModel>>;
    public class Handler : IRequestHandler<Query, IEnumerable<SpaceApiModel>>
    {
        private readonly CoreContext _context;
        private readonly IAuthenticationService _authenticationService;
        private readonly ISpaceAuthorizationService _spaceAuthorizationService;

        public Handler(CoreContext context, IAuthenticationService authenticationService, ISpaceAuthorizationService spaceAuthorizationService)
        {
            _context = context;
            _authenticationService = authenticationService;
            _spaceAuthorizationService = spaceAuthorizationService;
        }

        public async Task<IEnumerable<SpaceApiModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            var userId = _authenticationService.GetUserId();

            var spacesResult = await _spaceAuthorizationService.ApplyVisibilityFilter(_context.Spaces, request.InstitutionId, userId);

            return await spacesResult.GetOrThrow()
                .MapWith(SpaceApiModel.Mapper)
                .ToArrayAsync(cancellationToken);
        }
    }
}
