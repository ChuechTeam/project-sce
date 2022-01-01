using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Groups.ApiModels;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;

namespace Chuech.ProjectSce.Core.API.Features.Groups;

public static class CreateGroup
{
    public record Command(string Name, [property: JsonIgnore] int InstitutionId, int[]? UserIds = null)
        : IRequest<GroupApiModel>
    {
        public int[] UserIds { get; } = UserIds ?? Array.Empty<int>();
    }

    public class Handler : IRequestHandler<Command, GroupApiModel>
    {
        private readonly CoreContext _coreContext;
        private readonly AuthBarrier<InstitutionAuthorizationService> _authBarrier;
        private readonly GroupUserPresenceValidator _groupUserPresenceValidator;

        public Handler(CoreContext coreContext, AuthBarrier<InstitutionAuthorizationService> authBarrier,
            GroupUserPresenceValidator groupUserPresenceValidator)
        {
            _coreContext = coreContext;
            _authBarrier = authBarrier;
            _groupUserPresenceValidator = groupUserPresenceValidator;
        }

        public async Task<GroupApiModel> Handle(Command request, CancellationToken cancellationToken)
        {
            _ = await _authBarrier.GetAuthorizedUserIdAsync(
                (auth, userId) => auth.AuthorizeAsync(request.InstitutionId, userId, InstitutionPermission.ManageGroups)
            );

            if ((await _groupUserPresenceValidator.MayBePresent(request.UserIds, request.InstitutionId))
                .Any(x => x.Value is false))
            {
                throw Group.Errors.UserCannotEnter.AsException();
            }

            var group = new Group(request.InstitutionId, request.Name);
            _coreContext.Groups.Add(group);

            foreach (var groupUserId in request.UserIds)
            {
                _coreContext.GroupUsers.Add(new GroupUser(group, groupUserId));
            }

            await _coreContext.SaveChangesAsync(cancellationToken);

            return group.MapWith(GroupApiModel.Mapper(includeUsers: false));
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