using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.ApiModels;
using Chuech.ProjectSce.Core.API.Features.Institutions.Members;
using Chuech.ProjectSce.Core.API.Features.Users;
using MassTransit;

namespace Chuech.ProjectSce.Core.API.Features.Institutions;

public static class CreateInstitution
{
    public record Command(string Name) : IRequest<InstitutionApiModel>;

    public class Handler : IRequestHandler<Command, InstitutionApiModel>
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly CoreContext _coreContext;

        public Handler(IAuthenticationService authenticationService, CoreContext coreContext)
        {
            _authenticationService = authenticationService;
            _coreContext = coreContext;
        }
            
        public async Task<InstitutionApiModel> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = _authenticationService.GetUserId();

            // TODO: Provide a way to choose the educational role
            var institution = new Institution(request.Name);
            var institutionMember = new InstitutionMember(userId, institution, 
                InstitutionRole.Admin, EducationalRole.None);
                
            institution.Members.Add(institutionMember);
            _coreContext.Institutions.Add(institution);
            await _coreContext.SaveChangesAsync(cancellationToken);

            return institution.MapWith(InstitutionApiModel.Mapper);
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name).NotEmpty()
                .WithErrorCode("institution.name.required")
                .WithMessage("The institution name is required.");

            RuleFor(x => x.Name).MaximumLength(80)
                .WithErrorCode("institution.name.maxLengthExceeded")
                .WithMessage("The institution name must not exceed 80 characters.");
        }
    }
}