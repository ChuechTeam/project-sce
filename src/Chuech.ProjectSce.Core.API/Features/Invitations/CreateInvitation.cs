using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Invitations.ApiModels;
using Chuech.ProjectSce.Core.API.Features.Users;

namespace Chuech.ProjectSce.Core.API.Features.Invitations;

public static class CreateInvitation
{
    [UseInstitutionAuthorization(InstitutionPermission.InvitePeople)]
    public record Command(
        [property: JsonIgnore] int InstitutionId,
        Duration Validity,
        int Usages) : IRequest<DetailedInvitationApiModel>, IInstitutionRequest;

    public class Handler : IRequestHandler<Command, DetailedInvitationApiModel>
    {
        private readonly CoreContext _coreContext;
        private readonly IAuthenticationService _authenticationService;
        private readonly IClock _clock;

        public Handler(CoreContext coreContext, IAuthenticationService authenticationService, IClock clock)
        {
            _coreContext = coreContext;
            _authenticationService = authenticationService;
            _clock = clock;
        }

        public async Task<DetailedInvitationApiModel> Handle(Command request,
            CancellationToken cancellationToken)
        {
            var userId = _authenticationService.GetUserId();
                
            var invitation = new Invitation(
                canonicalId: Invitation.GenerateUniversalInvitationId(),
                institutionId: request.InstitutionId,
                creatorId: userId,
                usagesLeft: request.Usages,
                expirationDate: _clock.GetCurrentInstant() + request.Validity);
                
            _coreContext.Invitations.Add(invitation);
            await _coreContext.SaveChangesAsync(cancellationToken);

            await _coreContext.Entry(invitation).Reference(x => x.Creator).LoadAsync(cancellationToken);
            return invitation.MapWith(DetailedInvitationApiModel.Mapper);
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Usages)
                .InclusiveBetween(1, 1000)
                .WithMessage("The usages must be between 1 and 1000.")
                .WithErrorCode("institution.invitation.usages.invalid");

            RuleFor(x => x.Validity)
                .Must(x => x >= Duration.FromDays(1) && x <= Duration.FromDays(365))
                .WithMessage("The validity must be between 1 hour and 365 days.")
                .WithErrorCode("institution.invitation.validity.invalid");
        }
    }
}