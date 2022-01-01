using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Resources.ApiModels;
using Chuech.ProjectSce.Core.API.Features.Resources.Authorization;
using Chuech.ProjectSce.Core.API.Features.Users;

namespace Chuech.ProjectSce.Core.API.Features.Resources;

public static class GetResources
{
    public record Query(ResourceType? Type, int InstitutionId, int Start = 0, int Limit = 20)
        : IRequest<IEnumerable<ResourceApiModel>>;

    public class Handler : IRequestHandler<Query, IEnumerable<ResourceApiModel>>
    {
        private readonly CoreContext _context;
        private readonly IAuthenticationService _authenticationService;
        private readonly ResourceAuthorizationService _resourceAuthorizationService;

        public Handler(CoreContext context, ResourceAuthorizationService resourceAuthorizationService,
            IAuthenticationService authenticationService)
        {
            _context = context;
            _resourceAuthorizationService = resourceAuthorizationService;
            _authenticationService = authenticationService;
        }

        public async Task<IEnumerable<ResourceApiModel>> Handle(Query request,
            CancellationToken cancellationToken)
        {
            var userId = _authenticationService.GetUserId();
            var baseQuery = _context.Resources.AsQueryable();

            baseQuery = (await _resourceAuthorizationService.ApplyVisibilityFilter(baseQuery, request.InstitutionId,
                userId)).GetOrThrow();

            if (request.Type != null)
            {
                baseQuery = baseQuery.Where(x => x.Type == request.Type);
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