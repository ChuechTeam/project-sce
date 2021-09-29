using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Subjects.ApiModels;

namespace Chuech.ProjectSce.Core.API.Features.Subjects
{
    public static class GetSubjects
    {
        [UseInstitutionAuthorization]
        public record Query(int InstitutionId) : IRequest<IEnumerable<SubjectApiModel>>, IInstitutionRequest;

        public class Handler : IRequestHandler<Query, IEnumerable<SubjectApiModel>>
        {
            private readonly CoreContext _coreContext;

            public Handler(CoreContext coreContext)
            {
                _coreContext = coreContext;
            }

            public async Task<IEnumerable<SubjectApiModel>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _coreContext.Subjects
                    .Where(x => x.InstitutionId == request.InstitutionId)
                    .MapWith(SubjectApiModel.Mapper)
                    .ToListAsync(cancellationToken);
            }
        }
    }
}