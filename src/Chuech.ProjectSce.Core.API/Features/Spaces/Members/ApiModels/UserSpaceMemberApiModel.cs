using Chuech.ProjectSce.Core.API.Features.Users.ApiModels;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members.ApiModels;

public class UserSpaceMemberApiModel
{
    public UserApiModel User { get; set; } = null!;
    public SpaceMemberCategory Category { get; set; }

    public static readonly Mapper<UserSpaceMember, UserSpaceMemberApiModel> Mapper
        = new(x => new UserSpaceMemberApiModel
        {
            User = x.User.MapWith(UserApiModel.Mapper),
            Category = x.Category
        });
}

