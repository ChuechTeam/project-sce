using Chuech.ProjectSce.Core.API.Data;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Members.ApiModels
{
    public class InstitutionMemberApiModel
    {
        public static readonly Mapper<InstitutionMember, InstitutionMemberApiModel> Mapper = new(x => new InstitutionMemberApiModel
        {
            UserId = x.UserId,
            Name = x.User.DisplayName
        });

        public int UserId { get; set; }
        public string Name { get; set; } = null!;
    }
}