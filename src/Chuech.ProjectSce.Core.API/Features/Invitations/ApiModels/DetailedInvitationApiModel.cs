using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Users.ApiModels;

namespace Chuech.ProjectSce.Core.API.Features.Invitations.ApiModels
{
    public class DetailedInvitationApiModel
    {
        public static readonly Mapper<Invitation, DetailedInvitationApiModel> Mapper
            = new(x => new DetailedInvitationApiModel
            {
                Id = x.Id,
                CanonicalId = x.CanonicalId,
                Creator = x.Creator.MapWith(UserApiModel.Mapper),
                UsagesLeft = x.UsagesLeft,
                ExpirationDate = x.ExpirationDate,
                CreationDate = x.CreationDate
            });

        public string Id { get; set; } = null!;
        public string CanonicalId { get; set; } = null!;

        public int UsagesLeft { get; set; }

        public UserApiModel Creator { get; set; } = null!;

        public DateTime ExpirationDate { get; set; }
        public DateTime CreationDate { get; set; }
    }
}