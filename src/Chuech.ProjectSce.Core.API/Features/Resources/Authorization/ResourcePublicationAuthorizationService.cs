using Chuech.ProjectSce.Core.API.Features.Spaces.Authorization;

namespace Chuech.ProjectSce.Core.API.Features.Resources.Authorization;

public class ResourcePublicationAuthorizationService
{
    private readonly ResourceAuthorizationService _resourceAuthorizationService;
    private readonly SpaceAuthorizationService _spaceAuthorizationService;

    public ResourcePublicationAuthorizationService(ResourceAuthorizationService resourceAuthorizationService,
        SpaceAuthorizationService spaceAuthorizationService)
    {
        _resourceAuthorizationService = resourceAuthorizationService;
        _spaceAuthorizationService = spaceAuthorizationService;
    }

    public async Task<AuthorizationResult> AuthorizeAsync(Guid resourceId, IEnumerable<int> spaceIds, int userId)
    {
        var resourceAuthResult =
            await _resourceAuthorizationService.AuthorizeAsync(resourceId, userId, requiresAdminRights: true);
        if (!resourceAuthResult.IsSuccess)
        {
            return resourceAuthResult;
        }

        var spaceAuthResults = await _spaceAuthorizationService.AuthorizeManyAsync(spaceIds, userId);
        if (spaceAuthResults.Values.Any(x => !x.IsSuccess))
        {
            return AuthorizationResult.AwareOther(Resource.Errors.PublicationImpossible);
        }

        return AuthorizationResult.Success;
    }
}