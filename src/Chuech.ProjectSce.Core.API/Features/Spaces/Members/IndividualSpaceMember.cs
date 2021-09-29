using Chuech.ProjectSce.Core.API.Data;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Members;

public record IndividualSpaceMember(int SpaceId, int UserId, SpaceMemberCategory Category);
