using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using EntityFramework.Exceptions.Common;
using System.Text.Json.Serialization;

namespace Chuech.ProjectSce.Core.API.Features.Groups;
public static class UpdateGroup
{
    public record Command([property: JsonIgnore] int GroupId, string Name, int[]? UserIds = null) : IRequest
    {
        public int[] UserIds { get; } = UserIds ?? Array.Empty<int>();
    }
    public class Handler : AsyncRequestHandler<Command>
    {
        private readonly CoreContext _coreContext;
        private readonly AccessibleUsersFromIdsQuery _accessibleUsersQuery;
        private readonly AuthBarrier<IInstitutionAuthorizationService> _authBarrier;

        public Handler(CoreContext coreContext, 
            AccessibleUsersFromIdsQuery accessibleUsersQuery, 
            AuthBarrier<IInstitutionAuthorizationService> authBarrier)
        {
            _coreContext = coreContext;
            _accessibleUsersQuery = accessibleUsersQuery;
            _authBarrier = authBarrier;
        }

        protected async override Task Handle(Command request, CancellationToken cancellationToken)
        {
            var group = await _coreContext.Groups.FirstOrDefaultAsync(x => x.Id == request.GroupId, cancellationToken);
            if (group is null)
            {
                throw new NotFoundException();
            }

            var userId = _authBarrier.GetAuthorizedUserIdAsync(
                (auth, userId) => auth.AuthorizeAsync(group.InstitutionId, userId, InstitutionPermission.ManageGroups)
            );

            // Let's update
            // TODO: Concurrency
            group.Name = request.Name;
            await group.UpdateUsersAsync(request.UserIds, _accessibleUsersQuery);

            try
            {
                await _coreContext.SaveChangesAsync(cancellationToken);
            }
            catch (UniqueConstraintException)
            {
                throw GroupErrors.NameTakenError.AsException();
            }
        }
    }
}
