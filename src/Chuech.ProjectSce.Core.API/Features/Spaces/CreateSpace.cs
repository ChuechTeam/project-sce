using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Spaces.ApiModels;

namespace Chuech.ProjectSce.Core.API.Features.Spaces;
public static class CreateSpace
{
    public record Command(string Name, int SubjectId, int InstitutionId) : IRequest<SpaceApiModel>;

    public class Handler : IRequestHandler<Command, SpaceApiModel>
    {
        private readonly CoreContext _coreContext;
        private readonly AuthBarrier<InstitutionAuthorizationService> _authBarrier;

        public Handler(CoreContext coreContext, AuthBarrier<InstitutionAuthorizationService> authBarrier)
        {
            _coreContext = coreContext;
            _authBarrier = authBarrier;
        }

        public async Task<SpaceApiModel> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = await _authBarrier.GetAuthorizedUserIdAsync(
                (auth, userId) => auth.AuthorizeAsync(request.InstitutionId, userId, InstitutionPermission.CreateSpaces)
            );

            var subject = await _coreContext.Subjects
                .Where(x => x.Id == request.SubjectId && x.InstitutionId == request.InstitutionId)
                .FirstOrDefaultAsync(cancellationToken);
            if (subject is null)
            {
                throw new NotFoundException("Subject not found.");
            }

            var space = new Space(request.InstitutionId, request.Name, request.SubjectId, managerId: userId);
            _coreContext.Spaces.Add(space);

            await _coreContext.SaveChangesAsync(cancellationToken);

            _coreContext.Entry(space).Reference(x => x.Subject).Load();
            return space.MapWith(SpaceApiModel.Mapper);
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithErrorCode("space.name.required")
                .WithMessage("The name is required.");

            RuleFor(x => x.Name)
                .MaximumLength(50)
                .WithErrorCode("space.name.maxLengthExceeded")
                .WithMessage("The name must not be longer than 50 characters.");
        }
    }
}
