using Chuech.ProjectSce.Core.API.Data;

namespace Chuech.ProjectSce.Core.API.Features.Resources.Authorization;

public interface IResourceAuthorizationService
{
    Task<AuthorizationResult> AuthorizeAsync(int resourceId, int userId);
}

public class ResourceAuthorizationService : IResourceAuthorizationService
{
    private readonly CoreContext _coreContext;

    public ResourceAuthorizationService(CoreContext coreContext)
    {
        _coreContext = coreContext;
    }

    public async Task<AuthorizationResult> AuthorizeAsync(int resourceId, int userId)
    {
        var resource = await _coreContext.Resources.FindAsync(resourceId);
        if (resource?.AuthorId == userId)
        {
            return AuthorizationResult.Success;
        }
        return AuthorizationResult.HiddenForbidden("Resource not found.");
    }
}
