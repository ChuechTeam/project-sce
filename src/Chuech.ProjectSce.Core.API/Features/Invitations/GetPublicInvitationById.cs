using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Invitations.ApiModels;

namespace Chuech.ProjectSce.Core.API.Features.Invitations
{
    public static class GetPublicInvitationById
    {
        public record Query(string InvitationId) : IRequest<PublicInvitationApiModel?>;

        public class Handler : IRequestHandler<Query, PublicInvitationApiModel?>
        {
            private readonly CoreContext _coreContext;

            public Handler(CoreContext coreContext)
            {
                _coreContext = coreContext;
            }

            public async Task<PublicInvitationApiModel?> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var normalizedId = Invitation.NormalizeId(request.InvitationId);

                return await _coreContext.Invitations
                    .FilterValid()
                    .Where(x => x.Id == normalizedId)
                    .MapWith(PublicInvitationApiModel.Mapper)
                    .FirstOrDefaultAsync(cancellationToken);
            }
        }
    }
}