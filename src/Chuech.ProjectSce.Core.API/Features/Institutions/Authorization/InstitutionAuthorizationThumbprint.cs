using System.Collections.Immutable;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;

public record InstitutionAuthorizationThumbprint(
    int InstitutionId,
    int UserId,
    IReadOnlyCollection<InstitutionPermission> Permissions);