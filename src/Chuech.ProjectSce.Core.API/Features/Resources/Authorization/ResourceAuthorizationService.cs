using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Spaces.Members;

namespace Chuech.ProjectSce.Core.API.Features.Resources.Authorization;

public class ResourceAuthorizationService
{
    private readonly CoreContext _coreContext;
    private readonly InstitutionAuthorizationService _institutionAuthorizationService;

    public ResourceAuthorizationService(CoreContext coreContext,
        InstitutionAuthorizationService institutionAuthorizationService)
    {
        _coreContext = coreContext;
        _institutionAuthorizationService = institutionAuthorizationService;
    }

    public async Task<AuthorizationResult> AuthorizeAsync(Guid resourceId, int userId, bool requiresAdminRights = false)
    {
        var resourceAuthInfo = await _coreContext.Resources
            .Where(x => x.Id == resourceId)
            .Select(x => new
            {
                x.AuthorId,
                x.InstitutionId,
                AccessibleWithPublications = x.PublicationLocations
                    .Any(rp => rp.ResourceId == resourceId &&
                               (rp.Space.Members.OfType<UserSpaceMember>().Any(um => um.UserId == userId) ||
                                rp.Space.Members.OfType<GroupSpaceMember>()
                                    .Any(gm => gm.Group.Users.Any(u => u.Id == userId))))
            })
            .FirstOrDefaultAsync();
        if (resourceAuthInfo is null)
        {
            return AuthorizationResult.HiddenForbidden;
        }

        if (resourceAuthInfo.AuthorId == userId)
        {
            return AuthorizationResult.Success; // The author has admin rights anyway
        }

        if (resourceAuthInfo.AccessibleWithPublications && !requiresAdminRights)
        {
            return AuthorizationResult.Success;
        }

        // The user either:
        // - does not have direct access to the resource with publications but
        //   can get access to it with institution permissions 
        // - has access to the resource but needs institution permissions for
        //   admin rights on the resource

        var canManageResources = (await _institutionAuthorizationService.AuthorizeAsync(resourceAuthInfo.InstitutionId,
            userId, InstitutionPermission.ManageResources)).IsSuccess;

        if (!canManageResources)
        {
            if (resourceAuthInfo.AccessibleWithPublications)
            {
                return AuthorizationResult.AwareForbidden(Resource.Errors.PermissionMissing);
            }
            else
            {
                return AuthorizationResult.HiddenForbidden;
            }
        }

        return AuthorizationResult.Success;
    }

    public async Task<OperationResult<IQueryable<Resource>>> ApplyVisibilityFilter(IQueryable<Resource> resources,
        int institutionId, int userId)
    {
        resources = resources.Where(x => x.InstitutionId == institutionId);

        var authorizationResult = await _institutionAuthorizationService.AuthorizeAsync(institutionId, userId,
            InstitutionPermission.ManageResources);

        if (authorizationResult is HiddenForbiddenAuthorizationResult hiddenAuthorizationResult)
        {
            return OperationResult.Failure<IQueryable<Resource>>(hiddenAuthorizationResult.Error);
        }

        var hasManageResourcesPermission = authorizationResult.IsSuccess;
        if (!hasManageResourcesPermission)
        {
            resources = resources.Where(x => x.PublicationLocations
                .Any(rp => rp.ResourceId == x.Id &&
                           (rp.Space.Members.OfType<UserSpaceMember>().Any(um => um.UserId == userId) ||
                            rp.Space.Members.OfType<GroupSpaceMember>()
                                .Any(gm => gm.Group.Users.Any(u => u.Id == userId)))));
        }

        return OperationResult.Success(resources);
    }
}