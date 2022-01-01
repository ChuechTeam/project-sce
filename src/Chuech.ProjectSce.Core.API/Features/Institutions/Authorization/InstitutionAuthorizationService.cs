using System.Collections.Immutable;
using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.Members;
using Polly;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;

public class InstitutionAuthorizationService
{
    private readonly CoreContext _coreContext;
    private readonly IInstitutionAuthorizationCache _institutionAuthorizationCache;
    private readonly ChuechPolicyRegistry _policyRegistry;

    public InstitutionAuthorizationService(CoreContext coreContext,
        IInstitutionAuthorizationCache institutionAuthorizationCache, ChuechPolicyRegistry policyRegistry)
    {
        _coreContext = coreContext;
        _institutionAuthorizationCache = institutionAuthorizationCache;
        _policyRegistry = policyRegistry;
    }

    public async Task<AuthorizationResult> AuthorizeAsync(int institutionId, int userId,
        params InstitutionPermission[] requiredPermissions)
    {
        var thumbprint = await GetThumbprintAsync(institutionId, userId);
        return Authorize(thumbprint, requiredPermissions);
    }

    public AuthorizationResult Authorize(InstitutionAuthorizationThumbprint? thumbprint,
        params InstitutionPermission[] requiredPermissions)
    {
        if (thumbprint is null)
        {
            // They do not have access to the institution.
            return AuthorizationResult.HiddenForbidden;
        }

        if (requiredPermissions.Length == 0)
        {
            return AuthorizationResult.Success;
        }

        var acquiredPermissions = thumbprint.Permissions;
        var missingPermissions = requiredPermissions.Except(acquiredPermissions).ToArray();

        if (missingPermissions.Length != 0)
        {
            return AuthorizationResult.AwareForbidden(
                Institution.Errors.PermissionMissing with
                {
                    AdditionalInfo = new
                    {
                        Permissions = missingPermissions
                    }
                });
        }

        return AuthorizationResult.Success;
    }

    public async Task<InstitutionAuthorizationThumbprint?> GetThumbprintAsync(int institutionId, int userId)
    {
        Task<InstitutionAuthorizationThumbprint?> GetFromCacheAsync()
        {
            var policy = _policyRegistry.GetCacheFailureIgnorePolicy<InstitutionAuthorizationThumbprint>();
            var context = new Context("institution-auth-service:get-cached-thumbprint");

            return policy.ExecuteAsync(_ => _institutionAuthorizationCache.GetAsync(institutionId, userId), context);
        }
        
        Task UpdateCacheAsync(InstitutionAuthorizationThumbprint thumbprint)
        {
            var policy = _policyRegistry.CacheFailureIgnorePolicy;
            var context = new Context("institution-auth-service:update-thumbprint-cache");

            return policy.ExecuteAsync(_ => _institutionAuthorizationCache.SetAsync(thumbprint), context);
        }

        var thumbprint = await GetFromCacheAsync();

        if (thumbprint is null) // Not cached!
        {
            thumbprint = await CalculateThumbprintAsync(institutionId, userId);
            if (thumbprint is not null)
            {
                await UpdateCacheAsync(thumbprint);
            }
        }

        return thumbprint;
    }

    private async Task<InstitutionAuthorizationThumbprint?> CalculateThumbprintAsync(int institutionId, int userId)
    {
        var member = await _coreContext.InstitutionMembers.FindByPairAsync(userId, institutionId);
        if (member is null)
        {
            return null;
        }

        var permissions = new HashSet<InstitutionPermission>();
        if (member.InstitutionRole is InstitutionRole.Admin)
        {
            permissions.UnionWith(Enum.GetValues<InstitutionPermission>());
        }
        else
        {
            if (member.EducationalRole is EducationalRole.Teacher)
            {
                permissions.Add(InstitutionPermission.CreateSpaces);
            }
        }

        return new InstitutionAuthorizationThumbprint(
            member.InstitutionId,
            member.UserId,
            permissions);
    }
}