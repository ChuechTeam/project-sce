namespace Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;

/// <summary>
/// Indicates that authorization-related information may have changed for the specified users.
/// </summary>
/// <param name="InstitutionId">The institution id.</param>
/// <param name="UserIds">The users affected by the authorization change.</param>
public record InstitutionAuthorizationChanged(int InstitutionId, IReadOnlyCollection<int> UserIds);