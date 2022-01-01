namespace Chuech.ProjectSce.Core.API.Features.Institutions;

public enum InstitutionRole
{
    Admin,
    None
}

public static class InstitutionRoleExtensions
{
    public static bool IsHigherThan(this InstitutionRole a, InstitutionRole b) => a < b;
}