using Chuech.ProjectSce.Core.API.Features.Groups.ApiModels;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members.ApiModels;

public class GroupSpaceMemberApiModel
{
    public GroupApiModel Group { get; set; } = null!;
    public SpaceMemberCategory Category { get; set; }

    public static readonly Mapper<GroupSpaceMember, GroupSpaceMemberApiModel> Mapper
        = new(x => new GroupSpaceMemberApiModel
        {
            Group = x.Group.MapWith(GroupApiModel.Mapper(true)),
            Category = x.Category
        });
}