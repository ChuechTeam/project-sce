using Chuech.ProjectSce.Core.API.Features.Spaces.Members;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Authorization;

public record SpaceUserAuthorizationInfo(int UserId, SpaceMemberCategory? IndividualCategory, bool HasManageAllSpacesPermission);