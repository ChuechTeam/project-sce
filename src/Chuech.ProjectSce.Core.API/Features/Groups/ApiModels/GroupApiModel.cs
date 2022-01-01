using Chuech.ProjectSce.Core.API.Features.Users.ApiModels;
using System.Text.Json.Serialization;

namespace Chuech.ProjectSce.Core.API.Features.Groups.ApiModels;

public class GroupApiModel
{
    public int Id { get; set; }
    public int InstitutionId { get; set; }
    public string Name { get; set; } = null!;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<UserApiModel>? Users { get; set; }

    public static Mapper<Group, GroupApiModel> Mapper(bool includeUsers) => new(x => new GroupApiModel
    {
        Id = x.Id,
        InstitutionId = x.InstitutionId,
        Name = x.Name,
        Users = includeUsers ? x.Users.MapWith(UserApiModel.Mapper) : null
    });
}
