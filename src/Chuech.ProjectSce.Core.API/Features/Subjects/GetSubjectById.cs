using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Subjects.ApiModels;

namespace Chuech.ProjectSce.Core.API.Features.Subjects;

public static class GetSubjectById
{
    [UseInstitutionAuthorization]
    public record Query(int InstitutionId, int SubjectId) : IRequest<SubjectApiModel?>, IInstitutionRequest;

    public class Handler : IRequestHandler<Query, SubjectApiModel?>
    {
        private readonly CoreContext _coreContext;

        public Handler(CoreContext coreContext)
        {
            _coreContext = coreContext;
        }

        public async Task<SubjectApiModel?> Handle(Query request, CancellationToken cancellationToken)
        {
            return await _coreContext.Subjects
                .Where(x => x.Id == request.SubjectId && x.InstitutionId == request.InstitutionId)
                .MapWith(SubjectApiModel.Mapper)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}