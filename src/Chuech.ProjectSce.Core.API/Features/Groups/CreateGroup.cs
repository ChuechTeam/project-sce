using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Groups.ApiModels;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using EntityFramework.Exceptions.Common;
using Newtonsoft.Json;

namespace Chuech.ProjectSce.Core.API.Features.Groups;

public static class CreateGroup
{
    public record Command(string Name, [property: JsonIgnore] int InstitutionId, int[]? UserIds = null) : IRequest<GroupApiModel>
    {
        public int[] UserIds { get; } = UserIds ?? Array.Empty<int>();
    }
    public class Handler : IRequestHandler<Command, GroupApiModel>
    {
        private readonly CoreContext _coreContext;
        private readonly AccessibleUsersFromIdsQuery _accessibleUsersQuery;
        private readonly AuthBarrier<IInstitutionAuthorizationService> _authBarrier;

        public Handler(CoreContext coreContext, AccessibleUsersFromIdsQuery accessibleUsersQuery, 
            AuthBarrier<IInstitutionAuthorizationService> authBarrier)
        {
            _coreContext = coreContext;
            _accessibleUsersQuery = accessibleUsersQuery;
            _authBarrier = authBarrier;
        }

        public async Task<GroupApiModel> Handle(Command request, CancellationToken cancellationToken)
        {
            var userId = await _authBarrier.GetAuthorizedUserIdAsync(
                (auth, userId) => auth.AuthorizeAsync(request.InstitutionId, userId, InstitutionPermission.ManageGroups)
            );

            var group = new Group(request.InstitutionId, request.Name);
            await group.UpdateUsersAsync(request.UserIds, _accessibleUsersQuery);
            _coreContext.Groups.Add(group);

            try
            {
                await _coreContext.SaveChangesAsync(cancellationToken);
            } catch (UniqueConstraintException)
            {
                throw GroupErrors.NameTakenError.AsException();
            }

            return group.MapWith(GroupApiModel.Mapper(includeUsers: true));
        }
    }
    public class Validator : GroupValidator<Command>
    {
        public Validator()
        {
            AddNameRule(x => x.Name);
            AddUserIdsRule(x => x.UserIds);
        }
    }
}
