using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Groups.ApiModels;
using Chuech.ProjectSce.Core.API.Features.Institutions;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Users;

namespace Chuech.ProjectSce.Core.API.Features.Groups;
public static class GetGroupById
{
    public record Query(int GroupId) : IRequest<GroupApiModel?>;
    public class Handler : IRequestHandler<Query, GroupApiModel?>
    {
        private readonly CoreContext _coreContext;
        private readonly IAuthenticationService _authenticationService;

        public Handler(CoreContext coreContext, IAuthenticationService authenticationService)
        {
            _coreContext = coreContext;
            _authenticationService = authenticationService;
        }

        public async Task<GroupApiModel?> Handle(Query request, CancellationToken cancellationToken)
        {
            // TODO: Security, should some users not be able to view groups?
            var userId = _authenticationService.GetUserId();
            return await _coreContext.Groups
                .Where(x => x.Id == request.GroupId && x.Institution.Members.Any(m => m.UserId == userId))
                .MapWith(GroupApiModel.Mapper(true))
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
