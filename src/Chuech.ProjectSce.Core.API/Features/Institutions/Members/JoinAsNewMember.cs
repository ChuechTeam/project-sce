using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Invitations;
using Chuech.ProjectSce.Core.API.Features.Users;
using EntityFramework.Exceptions.Common;
using Newtonsoft.Json;
using Polly;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Members;

public static class JoinAsNewMember
{
    public record Command(
        [property: JsonIgnore] int InstitutionId,
        string InvitationId) : IRequest<Result>, IInstitutionRequest;

    public class Handler : IRequestHandler<Command, Result>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly CoreContext _coreContext;
        private readonly IInstitutionGatewayService _institutionGatewayService;
        private readonly ILogger<Handler> _logger;

        public Handler(CoreContext coreContext,
            IAuthenticationService authenticationService,
            ILogger<Handler> logger, IInstitutionGatewayService institutionGatewayService)
        {
            _coreContext = coreContext;
            _authenticationService = authenticationService;
            _logger = logger;
            _institutionGatewayService = institutionGatewayService;
        }

        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var (institutionId, invitationId) = request;
            var userId = _authenticationService.GetUserId();

            _logger.LogInformation(
                "Attempting to use invitation {InstitutionId} in institution {InstitutionId} by user {UserId}",
                invitationId, institutionId, userId);

            var concurrencyRetryPolicy = Policy
                .Handle<DbUpdateConcurrencyException>()
                .RetryAsync(5,
                    onRetry: (_, count) =>
                    {
                        _logger.LogInformation("Optimistic concurrency failure (retry #{RetryCount})", count);
                    });

            return await concurrencyRetryPolicy
                .ExecuteAsync(() => TryToJoin(invitationId, institutionId, userId, cancellationToken));
        }

        private async Task<Result> TryToJoin(string? invitationId, int institutionId, int userId,
            CancellationToken cancellationToken)
        {
            _coreContext.ChangeTracker.Clear();

            var invitation = await _coreContext.Invitations
                .FilterValid()
                .Where(x => x.Id == invitationId && x.InstitutionId == institutionId)
                .Include(x => x.Institution)
                .FirstOrDefaultAsync(cancellationToken);

            if (invitation is null)
            {
                _logger.LogInformation("Cannot use invitation, invitation not found or invalid");
                return Result.InviteNotFound;
            }

            var result = await JoinInstitution(invitation, userId, cancellationToken);

            if (result == Result.Success)
            {
                _logger.LogInformation(
                    "User {UserId} has joined institution {InstitutionId} with invitation {InvitationId}",
                    userId, institutionId, invitationId);
            }
            else if (result == Result.AlreadyPresent)
            {
                _logger.LogInformation("Cannot use invitation, member is already present");
            }

            return result;
        }

        private async Task<Result> JoinInstitution(Invitation invitation, int userId,
            CancellationToken cancellationToken)
        {
            invitation.ConsumeOneUsage();
            await _institutionGatewayService.JoinAsync(invitation.Institution, userId, InstitutionRole.None, EducationalRole.Student);

            try
            {
                await _coreContext.SaveChangesAsync(cancellationToken);
                return Result.Success;
            }
            catch (UniqueConstraintException)
            {
                return Result.AlreadyPresent;
            }
        }
    }

    public enum Result
    {
        Success,
        AlreadyPresent,
        InviteNotFound
    }
}
