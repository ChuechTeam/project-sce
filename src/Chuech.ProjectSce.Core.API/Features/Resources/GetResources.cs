using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Data.Resources;
using Chuech.ProjectSce.Core.API.Features.Resources.ApiModels;

namespace Chuech.ProjectSce.Core.API.Features.Resources
{
    public static class GetResources
    {
        public record Query(ResourceType? Type, int? InstitutionId = null, int Start = 0, int Limit = 20)
            : IRequest<IEnumerable<ResourceApiModel>>;
        
        public class Handler : IRequestHandler<Query, IEnumerable<ResourceApiModel>>
        {
            private readonly CoreContext _context;

            public Handler(CoreContext context)
            {
                _context = context;
            }

            public async Task<IEnumerable<ResourceApiModel>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var baseQuery = _context.Resources.AsQueryable();
                
                if (request.Type != null)
                {
                    baseQuery = baseQuery.Where(x => x.Type == request.Type);
                }

                if (request.InstitutionId != null)
                {
                    baseQuery = baseQuery.Where(x => x.InstitutionId == request.InstitutionId);
                }

                return await baseQuery
                    .OrderByDescending(x => x.CreationDate)
                    .MapWith(ResourceApiModel.PolymorphicMapper)
                    .Skip(request.Start)
                    .Take(request.Limit)
                    .ToArrayAsync(cancellationToken);
            }
        }

        public class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.Limit)
                    .InclusiveBetween(1, 50)
                    .WithMessage("The limit must be between 1 and 50.")
                    .WithErrorCode("resources.limit.invalid");

                RuleFor(x => x.Start)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("The start must be greater or equal to 0.")
                    .WithErrorCode("resources.start.invalid");
            }
        }
    }
}