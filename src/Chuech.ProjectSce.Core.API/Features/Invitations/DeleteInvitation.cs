using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;

namespace Chuech.ProjectSce.Core.API.Features.Invitations;

public static class DeleteInvitation
{
    [UseInstitutionAuthorization(InstitutionPermission.InvitePeople)]
    public record Command(int InstitutionId, string InvitationId) : IRequest, IInstitutionRequest;

    public class Handler : AsyncRequestHandler<Command>
    {
        private readonly CoreContext _coreContext;

        public Handler(CoreContext coreContext)
        {
            _coreContext = coreContext;
        }

        protected override async Task Handle(Command request, CancellationToken cancellationToken)
        {
            var id = Invitation.NormalizeId(request.InvitationId);

            var invitation = await _coreContext.Invitations.FindAsync(new object[] { id }, cancellationToken);
            if (invitation is null)
            {
                throw new NotFoundException("Invitation not found.");
            }

            _coreContext.Remove(invitation);
            await _coreContext.SaveChangesAsync(cancellationToken);
        }
    }
}