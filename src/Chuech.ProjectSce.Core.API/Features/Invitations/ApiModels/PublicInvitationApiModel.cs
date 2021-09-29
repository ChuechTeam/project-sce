using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.ApiModels;

namespace Chuech.ProjectSce.Core.API.Features.Invitations.ApiModels
{
    public class PublicInvitationApiModel
    {
        public static readonly Mapper<Invitation, PublicInvitationApiModel> Mapper = new(x =>
            new PublicInvitationApiModel
            {
                Id = x.Id,
                CanonicalId = x.CanonicalId,
                Institution = x.Institution.MapWith(InstitutionApiModel.Mapper)
            });

        public string Id { get; set; } = null!;
        public string CanonicalId { get; set; } = null!;

        public InstitutionApiModel Institution { get; set; } = null!;
    }
}