using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Institutions.Members.ApiModels;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Members;

public static class GetMembers
{
    [UseInstitutionAuthorization]
    public record Query(int InstitutionId) : IRequest<IEnumerable<InstitutionMemberApiModel>>, IInstitutionRequest;

    public class Handler : IRequestHandler<Query, IEnumerable<InstitutionMemberApiModel>>
    {
        private readonly CoreContext _coreContext;

        public Handler(CoreContext coreContext)
        {
            _coreContext = coreContext;
        }

        public async Task<IEnumerable<InstitutionMemberApiModel>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var members = await _coreContext.InstitutionMembers
                .Where(x => x.InstitutionId == request.InstitutionId)
                .MapWith(InstitutionMemberApiModel.Mapper)
                .ToArrayAsync(cancellationToken);

            return members;
        }
    }
}
