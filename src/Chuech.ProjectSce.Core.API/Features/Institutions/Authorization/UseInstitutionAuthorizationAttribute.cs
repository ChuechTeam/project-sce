namespace Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;

[AttributeUsage(AttributeTargets.Class)]
public class UseInstitutionAuthorizationAttribute : Attribute
{
    public UseInstitutionAuthorizationAttribute(params InstitutionPermission[] permissionsRequired)
    {
        PermissionsRequired = permissionsRequired;
    }

    public InstitutionPermission[] PermissionsRequired { get; }
}