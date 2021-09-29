using Chuech.ProjectSce.Core.API.Data;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Authorization
{
    public interface IInstitutionAuthorizationService
    {
        Task<IEnumerable<InstitutionPermission>> GetPermissionsAsync(InstitutionMember institutionMember);

        Task<AuthorizationResult> AuthorizeAsync(int institutionId, int userId, params InstitutionPermission[] requiredPermissions);
    }

    public class InstitutionAuthorizationService : IInstitutionAuthorizationService
    {
        private readonly CoreContext _coreContext;

        public InstitutionAuthorizationService(CoreContext coreContext)
        {
            _coreContext = coreContext;
        }

        public async Task<AuthorizationResult> AuthorizeAsync(int institutionId, int userId, params InstitutionPermission[] requiredPermissions)
        {
            var institutionMember = await _coreContext.InstitutionMembers.FindByPairAsync(userId, institutionId);
            if (institutionMember is null)
            {
                // They do not have access to the institution.
                return AuthorizationResult.HiddenForbidden();
            }

            if (requiredPermissions.Length == 0)
            {
                return AuthorizationResult.Success;
            }

            var acquiredPermissions = await GetPermissionsAsync(institutionMember);
            var missingPermissions = requiredPermissions.Except(acquiredPermissions).ToArray();

            if (missingPermissions.Length != 0)
            {
                return AuthorizationResult.AwareFobidden(
                    $"You do not have the permission to do this action ({string.Join(", ", missingPermissions)}).", 
                    InstitutionErrors.PermissionMissing);
            }
            return AuthorizationResult.Success;
        }

        public Task<IEnumerable<InstitutionPermission>> GetPermissionsAsync(InstitutionMember institutionMember)
        {
            if (institutionMember.InstitutionRole is InstitutionRole.Admin)
            {
                IEnumerable<InstitutionPermission> allPermissions = Enum.GetValues<InstitutionPermission>();
                return Task.FromResult(allPermissions);
            }

            var permissions = new HashSet<InstitutionPermission>();
            if (institutionMember.EducationalRole is EducationalRole.Teacher)
            {
                permissions.Add(InstitutionPermission.CreateSpaces);
            }

            return Task.FromResult<IEnumerable<InstitutionPermission>>(permissions);
        }
    }
}