using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Subjects.ApiModels;

namespace Chuech.ProjectSce.Core.API.Features.Subjects;

public static class CreateSubject
{
    [UseInstitutionAuthorization(InstitutionPermission.ManageSubjects)]
    public record Command([property: JsonIgnore] int InstitutionId, string Name, RgbColor Color)
        : IRequest<SubjectApiModel>, IInstitutionRequest;

    public class Handler : IRequestHandler<Command, SubjectApiModel>
    {
        private readonly CoreContext _coreContext;

        public Handler(CoreContext coreContext)
        {
            _coreContext = coreContext;
        }

        public async Task<SubjectApiModel> Handle(Command request, CancellationToken cancellationToken)
        {
            var subject = new Subject(request.InstitutionId, request.Name, request.Color);
            _coreContext.Subjects.Add(subject);
            await _coreContext.SaveChangesAsync(cancellationToken);

            return subject.MapWith(SubjectApiModel.Mapper);
        }
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            // TODO: Trim support
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithErrorCode("subject.name.required");

            RuleFor(x => x.Name)
                .MaximumLength(50)
                .WithErrorCode("subject.name.maxLengthExceeded");
        }
    }
}
