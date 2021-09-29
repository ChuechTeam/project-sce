using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Groups.ApiModels;
using Chuech.ProjectSce.Core.API.Features.Institutions;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;

namespace Chuech.ProjectSce.Core.API.Features.Groups;
public static class GetGroups
{
    [UseInstitutionAuthorization]
    public record Query(int InstitutionId, bool IncludeUsers) : IRequest<IEnumerable<GroupApiModel>>, IInstitutionRequest;
    public class Handler : IRequestHandler<Query, IEnumerable<GroupApiModel>>
    {
        private readonly CoreContext _coreContext;

        public Handler(CoreContext coreContext)
        {
            _coreContext = coreContext;
        }

        public async Task<IEnumerable<GroupApiModel>> Handle(Query request, CancellationToken cancellationToken)
        {
            // TODO: Security, should some users not be able to view groups?
            return await _coreContext.Groups
                .Where(x => x.InstitutionId == request.InstitutionId)
                .MapWith(GroupApiModel.Mapper(request.IncludeUsers))
                .ToArrayAsync(cancellationToken);
        }
    }
}
