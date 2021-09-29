using Chuech.ProjectSce.Core.API.Data;

namespace Chuech.ProjectSce.Core.API.Features.Users.ApiModels
{
    public class UserApiModel
    {
        public static readonly Mapper<User, UserApiModel> Mapper = new(x => new UserApiModel
        {
            Id = x.Id,
            DisplayName = x.DisplayName
        });

        public int Id { get; set; }
        public string DisplayName { get; set; } = null!;
    }
}