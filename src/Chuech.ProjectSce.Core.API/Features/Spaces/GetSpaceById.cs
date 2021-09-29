using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Spaces.ApiModels;
using Chuech.ProjectSce.Core.API.Features.Spaces.Authorization;
using Chuech.ProjectSce.Core.API.Features.Spaces.Members;
using Chuech.ProjectSce.Core.API.Features.Users;

namespace Chuech.ProjectSce.Core.API.Features.Spaces;

public static class GetSpaceById
{
    public record Query(int SpaceId) : IRequest<SpaceApiModel?>;
    public class Handler : IRequestHandler<Query, SpaceApiModel?>
    {
        private readonly CoreContext _context;
        private readonly AuthBarrier<ISpaceAuthorizationService> _authBarrier;

        public Handler(CoreContext context, AuthBarrier<ISpaceAuthorizationService> authBarrier)
        {
            _context = context;
            _authBarrier = authBarrier;
        }

        public async Task<SpaceApiModel?> Handle(Query request, CancellationToken cancellationToken)
        {
            var result = await _authBarrier.GetAuthorizedUserIdResultAsync(
                (auth, userId) => auth.AuthorizeAsync(request.SpaceId, userId)
            );

            if (!result.IsSuccess)
            {
                return null;
            }

            return await _context.Spaces
                .MapWith(SpaceApiModel.Mapper)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}