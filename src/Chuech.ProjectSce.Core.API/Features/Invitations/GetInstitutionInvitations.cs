using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Invitations.ApiModels;

namespace Chuech.ProjectSce.Core.API.Features.Invitations
{
    [UseInstitutionAuthorization(InstitutionPermission.InvitePeople)]
    public static class GetInstitutionInvitations
    {
        public record Query(int InstitutionId)
            : IRequest<IEnumerable<DetailedInvitationApiModel>>, IInstitutionRequest;
        
        public class Handler : IRequestHandler<Query, IEnumerable<DetailedInvitationApiModel>>
        {
            private readonly CoreContext _coreContext;

            public Handler(CoreContext coreContext)
            {
                _coreContext = coreContext;
            }

            public async Task<IEnumerable<DetailedInvitationApiModel>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                return await _coreContext.Invitations
                    .FilterValid()
                    .MapWith(DetailedInvitationApiModel.Mapper)
                    .ToArrayAsync(cancellationToken);
            }
        }
    }
}