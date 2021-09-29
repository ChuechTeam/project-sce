using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Users;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Members;

public static class KickMember
{
    // The permission is checked manually, as the user can kick themselves without
    // requiring the KickMember permission.
    [UseInstitutionAuthorization]
    public record Command(int InstitutionId, int UserToKickId)
        : IRequest, IInstitutionRequest;

    public class Handler : IRequestHandler<Command>
    {
        private readonly CoreContext _coreContext;
        private readonly ILogger<Handler> _logger;
        private readonly IInstitutionAuthorizationService _institutionAuthorizationService;
        private readonly IAuthenticationService _authenticationService;

        public Handler(CoreContext coreContext,
            ILogger<Handler> logger,
            IInstitutionAuthorizationService institutionAuthorizationService,
            IAuthenticationService authenticationService)
        {
            _coreContext = coreContext;
            _logger = logger;
            _institutionAuthorizationService = institutionAuthorizationService;
            _authenticationService = authenticationService;
        }

        public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
        {
            var kickerUserId = _authenticationService.GetUserId();
            var kicker = await _coreContext.InstitutionMembers.FindByPairAsync(kickerUserId, request.InstitutionId, cancellationToken)
                ?? throw new NotFoundException("Institution not found.");

            InstitutionMember userToKick;

            // The user is kicking themselves
            if (kicker.UserId == request.UserToKickId)
            {
                userToKick = kicker;

                if (userToKick.InstitutionRole == InstitutionRole.Admin)
                {
                    var adminCount = await _coreContext.InstitutionMembers
                        .CountAsync(x => x.InstitutionRole == InstitutionRole.Admin, cancellationToken);
                    if (adminCount == 1)
                    {
                        throw MemberErrors.CannotQuitAsLastAdminError.AsException();
                    }
                }
            }
            else
            {
                // The user is trying to kick someone else than themselves, so let's check
                // if they have permission to do so.
                await _institutionAuthorizationService.AuthorizeAsync(request.InstitutionId, kicker.UserId,
                    InstitutionPermission.ManageMembers).ThrowIfUnsuccessful();

                userToKick = await _coreContext.InstitutionMembers.FindByPairAsync(request.UserToKickId,
                    request.InstitutionId,
                    cancellationToken) ?? throw new NotFoundException($"User to kick not found.");

                // Check if the kicker has the right to kick someone in their hierarchy
                if (userToKick.InstitutionRole >= kicker.InstitutionRole)
                {
                    throw MemberErrors.CannotKickHigherInHierarchyError(kicker, userToKick).AsException();
                }
            }

            await KickUser(userToKick, cancellationToken);

            _logger.LogInformation(
                "User {UserId} has been kicked from the institution {InstitutionId} by user {KickerId}",
                request.UserToKickId, request.InstitutionId, kicker.UserId);

            return Unit.Value;
        }

        // TODO: Refactor this
        private async Task KickUser(InstitutionMember userToKick, CancellationToken cancellationToken)
        {
            _coreContext.InstitutionMembers.Remove(userToKick);
            await _coreContext.SaveChangesAsync(cancellationToken);
        }
    }
}
