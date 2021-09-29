using Chuech.ProjectSce.Core.API.Data;

namespace Chuech.ProjectSce.Core.API.Features.Institutions
{
    public static class InstitutionErrors
    {
        public const string PermissionMissing = "institution.permissionMissing";

        public static Error PermissionsMissingError(IEnumerable<InstitutionPermission> permissions) =>
            new($"You do not have the permission to do this action ({string.Join(",", permissions)}).",
                PermissionMissing, ErrorKind.AuthorizationFailure);
    }
}