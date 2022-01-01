using Chuech.ProjectSce.Core.API.Data;

namespace Chuech.ProjectSce.Core.API.Features.Resources;

public class ResourcePublicationValidator
{
    private readonly CoreContext _coreContext;

    public ResourcePublicationValidator(CoreContext coreContext)
    {
        _coreContext = coreContext;
    }

    public async Task<bool> CanBePublishedInSpacesAsync(Resource resource, IEnumerable<int> spaceIds)
    {
        var spaceIdsArray = spaceIds.ToArray();

        var count = await _coreContext.Spaces
            .CountAsync(x => x.InstitutionId == resource.InstitutionId && spaceIdsArray.Contains(x.Id));

        return count == spaceIdsArray.Length;
    }
}