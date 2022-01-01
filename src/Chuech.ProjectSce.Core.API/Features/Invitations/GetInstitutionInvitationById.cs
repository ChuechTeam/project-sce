using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Invitations.ApiModels;

namespace Chuech.ProjectSce.Core.API.Features.Invitations;

public static class GetInstitutionInvitationById
{
    [UseInstitutionAuthorization(InstitutionPermission.InvitePeople)]
    public record Query(int InstitutionId, string InvitationId)
        : IRequest<DetailedInvitationApiModel?>, IInstitutionRequest;

    public class Handler : IRequestHandler<Query, DetailedInvitationApiModel?>
    {
        private readonly CoreContext _coreContext;
        private IClock _clock;

        public Handler(CoreContext coreContext, IClock clock)
        {
            _coreContext = coreContext;
            _clock = clock;
        }

        public async Task<DetailedInvitationApiModel?> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var normalizedId = Invitation.NormalizeId(request.InvitationId);

            return await _coreContext.Invitations
                .FilterValid(_clock)
                .Where(x => x.Id == normalizedId)
                .MapWith(DetailedInvitationApiModel.Mapper)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
