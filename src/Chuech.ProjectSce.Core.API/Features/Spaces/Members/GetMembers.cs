using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Spaces.Authorization;
using Chuech.ProjectSce.Core.API.Features.Spaces.Members.ApiModels;
using NotSoAutoMapper.ExpressionProcessing;
using System.Linq.Expressions;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members;
public static class GetMembers
{
    public record Query(int SpaceId) : IRequest<Result>;
    public class Handler : IRequestHandler<Query, Result>
    {
        private readonly CoreContext _coreContext;
        private readonly AuthBarrier<ISpaceAuthorizationService> _authBarrier;

        public Handler(CoreContext coreContext, AuthBarrier<ISpaceAuthorizationService> authBarrier)
        {
            _coreContext = coreContext;
            _authBarrier = authBarrier;
        }

        private static readonly Expression<Func<Space, Result>> _spaceProjection = ((Expression<Func<Space, Result>>)(x => new Result
        {
            Groups = x.Members.OfType<GroupSpaceMember>().MapWith(GroupSpaceMemberApiModel.Mapper).ToArray(),
            Users = x.Members.OfType<UserSpaceMember>().MapWith(UserSpaceMemberApiModel.Mapper).ToArray(),
        })).ApplyTransformations();

        public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
        {
            _ = await _authBarrier.GetAuthorizedUserIdAsync(
                (auth, userId) => auth.AuthorizeAsync(request.SpaceId, userId)
            );

            return await _coreContext.Spaces
                .Where(x => x.Id == request.SpaceId)
                .Select(_spaceProjection)
                .FirstAsync(cancellationToken);
        }
    }
    public record Result
    {
        public GroupSpaceMemberApiModel[] Groups { get; init; } = null!;
        public UserSpaceMemberApiModel[] Users { get; init; } = null!;
    }
}
