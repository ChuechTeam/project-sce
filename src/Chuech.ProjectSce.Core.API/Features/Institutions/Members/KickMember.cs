using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Institutions.Members.Commands;
using Chuech.ProjectSce.Core.API.Features.Users;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Members;

public static class KickMember
{
    public record Command(int InstitutionId, int UserToKickId)
        : IRequest<OperationResult>, IInstitutionRequest;

    public class Handler : IRequestHandler<Command, OperationResult>
    {
        private readonly CoreContext _coreContext;
        private readonly InstitutionAuthorizationService _institutionAuthorizationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IRequestClient<RemoveInstitutionMember> _removeMemberClient;

        public Handler(CoreContext coreContext,
            InstitutionAuthorizationService institutionAuthorizationService,
            IAuthenticationService authenticationService,
            IRequestClient<RemoveInstitutionMember> removeMemberClient)
        {
            _coreContext = coreContext;
            _institutionAuthorizationService = institutionAuthorizationService;
            _authenticationService = authenticationService;
            _removeMemberClient = removeMemberClient;
        }

        public async Task<OperationResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var kickerId = _authenticationService.GetUserId();
            var (institutionId, userToKickId) = request;

            var kicker = await _coreContext.InstitutionMembers
                .Where(x => x.InstitutionId == institutionId && x.UserId == kickerId)
                .Select(x => new
                {
                    x.InstitutionRole
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (kicker is null)
            {
                return OperationResult.Failure(new Error(Kind: ErrorKind.NotFound));
            }

            var userToKick = await _coreContext.InstitutionMembers
                .Where(x => x.InstitutionId == institutionId && x.UserId == userToKickId)
                .Select(x => new
                {
                    x.InstitutionRole
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (userToKick is null)
            {
                return OperationResult.Failure(new Error("User to kick not found.", Kind: ErrorKind.NotFound));
            }

            if (userToKickId != kickerId)
            {
                // Make sure they are authorized to kick someone else than themselves.
                var authResult = await _institutionAuthorizationService.AuthorizeAsync(institutionId, kickerId,
                    InstitutionPermission.ManageMembers);
                if (authResult.Failed(out var authError))
                {
                    return OperationResult.Failure(authError);
                }
                
                if (userToKick.InstitutionRole.IsHigherThan(kicker.InstitutionRole))
                {
                    return OperationResult.Failure(InstitutionMember.Errors.CannotKickHigherInHierarchy);
                }
            }

            Response response = await _removeMemberClient
                .GetResponse(new RemoveInstitutionMember(institutionId, userToKickId), cancellationToken);

            return response switch
            {
                (_, RemoveInstitutionMember.Failure failure) => OperationResult.Failure(failure.Error),
                (_, RemoveInstitutionMember.Success) => OperationResult.Success(),
                _ => throw new InvalidOperationException("Unknown response type.")
            };
        }
    }
}