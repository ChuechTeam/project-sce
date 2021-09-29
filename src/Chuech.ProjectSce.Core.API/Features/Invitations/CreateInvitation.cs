using System.Text.Json.Serialization;
using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Invitations.ApiModels;
using Chuech.ProjectSce.Core.API.Features.Users;

namespace Chuech.ProjectSce.Core.API.Features.Invitations
{
    public static class CreateInvitation
    {
        [UseInstitutionAuthorization(InstitutionPermission.InvitePeople)]
        public record Command(
            [property: JsonIgnore] int InstitutionId,
            TimeSpan Validity,
            int Usages) : IRequest<DetailedInvitationApiModel>, IInstitutionRequest;

        public class Handler : IRequestHandler<Command, DetailedInvitationApiModel>
        {
            private readonly CoreContext _coreContext;
            private readonly IAuthenticationService _authenticationService;

            public Handler(CoreContext coreContext, IAuthenticationService authenticationService)
            {
                _coreContext = coreContext;
                _authenticationService = authenticationService;
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
                    expirationDate: DateTime.UtcNow + request.Validity);
                
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
                    .InclusiveBetween(TimeSpan.FromHours(1), TimeSpan.FromDays(365))
                    .WithMessage("The validity must be between 1 hour and 1 year.")
                    .WithErrorCode("institution.invitation.validity.invalid");
            }
        }
    }
}