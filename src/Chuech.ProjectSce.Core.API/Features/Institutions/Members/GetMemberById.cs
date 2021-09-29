using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Institutions.Members.ApiModels;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Members
{
    [UseInstitutionAuthorization]
    public static class GetMemberById
    {
        public record Query(int InstitutionId, int UserId)
            : IRequest<InstitutionMemberApiModel?>, IInstitutionRequest;

        public class Handler : IRequestHandler<Query, InstitutionMemberApiModel?>
        {
            private readonly CoreContext _coreContext;

            public Handler(CoreContext coreContext)
            {
                _coreContext = coreContext;
            }

            public async Task<InstitutionMemberApiModel?> Handle(Query request, CancellationToken cancellationToken)
            {
                var member = await _coreContext.InstitutionMembers
                    .Where(x => x.InstitutionId == request.InstitutionId)
                    .MapWith(InstitutionMemberApiModel.Mapper)
                    .FirstOrDefaultAsync(cancellationToken);

                return member;
            }
        }
    }
}