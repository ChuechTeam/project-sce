using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Spaces.Members;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Authorization;

public interface ISpaceAuthorizationService
{
    Task<AuthorizationResult> AuthorizeAsync(int spaceId, int userId, params SpacePermission[] requiredPermissions);
    Task<OperationResult<IQueryable<Space>>> ApplyVisibilityFilter(IQueryable<Space> spaces, int institutionId, int userId);
}
public sealed class SpaceAuthorizationService : ISpaceAuthorizationService
{
    private readonly CoreContext _coreContext;
    private readonly IInstitutionAuthorizationService _institutionAuthorizationService;
    private readonly IIndividualSpaceMemberCalculator _individualSpaceMemberCalculator;
    private readonly ISpaceUserAuthorizationInfoCache _authorizationInfoCache;

    public SpaceAuthorizationService(CoreContext coreContext,
        IInstitutionAuthorizationService institutionAuthorizationService,
        IIndividualSpaceMemberCalculator individualSpaceMemberCalculator, 
        ISpaceUserAuthorizationInfoCache authorizationInfoCache)
    {
        _coreContext = coreContext;
        _institutionAuthorizationService = institutionAuthorizationService;
        _individualSpaceMemberCalculator = individualSpaceMemberCalculator;
        _authorizationInfoCache = authorizationInfoCache;
    }

    public async Task<AuthorizationResult> AuthorizeAsync(int spaceId, int userId, params SpacePermission[] requiredPermissions)
    {
        var authData = await GetAuthDataAsync(spaceId, userId);

        if (authData is null)
        {
            return AuthorizationResult.HiddenForbidden();
        }

        var presentPermissions = GetPermissions(authData);
        var missingPermissions = requiredPermissions.Except(presentPermissions).ToArray();
        if (missingPermissions.Any())
        {
            // TODO: Show the missing permissions?
            return AuthorizationResult.AwareFobidden("You do not have permission to do this action.",
                "space.missingPermissions");
        }

        return AuthorizationResult.Success;
    }

    private async Task<SpaceUserAuthorizationInfo?> GetAuthDataAsync(int spaceId, int userId)
    {
        var info = (await _authorizationInfoCache.GetAsync(spaceId, userId)) ?? await CalculateAuthDataAsync(spaceId, userId);
        if (info is not null)
        {
            await _authorizationInfoCache.PutAsync(spaceId, info, TimeSpan.FromHours(12));
        }
        return info;
    }

    private async Task<SpaceUserAuthorizationInfo?> CalculateAuthDataAsync(int spaceId, int userId)
    {
        // TODO: Cache this!

        var institutionId = await _coreContext.Spaces
            .Where(x => x.Id == spaceId)
            .Select(x => x.InstitutionId)
            .FirstOrDefaultAsync();

        if (institutionId == default)
        {
            // Then the space doesn't exist
            return null;
        }

        var flatMembers = (await _coreContext.SpaceMembers
            .Where(x => x.SpaceId == spaceId)
            .Flattened()
            .ToListAsync())
            .Where(x => x.UserIds.Any(id => id == userId));

        var individualMember = _individualSpaceMemberCalculator.Calculate(flatMembers)
            .FirstOrDefault(x => x.UserId == userId);
        var hasManageAllSpacesPermission = (await _institutionAuthorizationService.AuthorizeAsync(institutionId, userId, 
            InstitutionPermission.ManageAllSpaces)).IsSuccess;

        return new SpaceUserAuthorizationInfo(userId, individualMember?.Category, hasManageAllSpacesPermission);
    }

    private static IEnumerable<SpacePermission> GetPermissions(SpaceUserAuthorizationInfo summary)
    {
        if (summary.HasManageAllSpacesPermission || summary.IndividualCategory is SpaceMemberCategory.Manager)
        {
            return Enum.GetValues<SpacePermission>();
        }

        return Array.Empty<SpacePermission>();
    }

    public async Task<OperationResult<IQueryable<Space>>> ApplyVisibilityFilter(IQueryable<Space> spaces, int institutionId, int userId)
    {
        spaces = spaces.Where(x => x.InstitutionId == institutionId);

        var authorizationResult = await _institutionAuthorizationService.AuthorizeAsync(institutionId, userId, 
            InstitutionPermission.ManageAllSpaces);

        if (authorizationResult is HiddenForbiddenAuthorizationResult hiddenAuthorizationResult)
        {
            return OperationResult.Failure<IQueryable<Space>>(hiddenAuthorizationResult.AsError());
        }

        var hasManageAllSpacesPermission = authorizationResult.IsSuccess;

        if (hasManageAllSpacesPermission)
        {
            return OperationResult.Success(spaces);
        }
        else
        {
            return OperationResult.Success(spaces.Where(x => x.Members.Any(m =>
            m is UserSpaceMember ?
                ((UserSpaceMember)m).UserId == userId :
            m is GroupSpaceMember ?
                ((GroupSpaceMember)m).Group.Users.Any(gu => gu.Id == userId) :
            false)));
        }
    }
}
