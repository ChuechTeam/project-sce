using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Data.Abstractions;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;

namespace Chuech.ProjectSce.Core.API.Features.Groups;

public static class AddUser
{
    public record Command([property: JsonIgnore] int GroupId, int UserId) : IRequest;

    public class Handler : IRequestHandler<Command>
    {
        private readonly CoreContext _coreContext;
        private readonly AuthBarrier<InstitutionAuthorizationService> _authBarrier;
        private readonly GroupUserPresenceValidator _groupUserPresenceValidator;

        public Handler(CoreContext coreContext,
            AuthBarrier<InstitutionAuthorizationService> authBarrier,
            GroupUserPresenceValidator groupUserPresenceValidator)
        {
            _coreContext = coreContext;
            _authBarrier = authBarrier;
            _groupUserPresenceValidator = groupUserPresenceValidator;
        }

        public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
        {
            var group = await _coreContext.AvailableGroups
                .FirstOrDefaultAsync(x => x.Id == command.GroupId, cancellationToken);

            if (group is null)
            {
                throw new NotFoundException();
            }

            _ = await _authBarrier.GetAuthorizedUserIdAsync(
                (auth, userId) => auth.AuthorizeAsync(group.InstitutionId, userId, InstitutionPermission.ManageGroups));

            if (!await _groupUserPresenceValidator.MayBePresent(command.UserId, group.InstitutionId))
            {
                throw Group.Errors.UserCannotEnter.AsException();
            }

            var groupUser = new GroupUser(group.Id, command.UserId);
            _coreContext.GroupUsers.Add(groupUser);

            await _coreContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}