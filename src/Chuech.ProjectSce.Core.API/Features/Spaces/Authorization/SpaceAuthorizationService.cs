using System.Collections.Immutable;
using Chuech.ProjectSce.Core.API.Data;
using Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;
using Chuech.ProjectSce.Core.API.Features.Spaces.Members;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Authorization;

public sealed class SpaceAuthorizationService
{
    private readonly CoreContext _coreContext;
    private readonly InstitutionAuthorizationService _institutionAuthorizationService;
    private readonly IndividualSpaceMemberCalculator _individualSpaceMemberCalculator;
    private readonly ISpaceAuthorizationCache _authorizationCache;

    public SpaceAuthorizationService(CoreContext coreContext,
        InstitutionAuthorizationService institutionAuthorizationService,
        IndividualSpaceMemberCalculator individualSpaceMemberCalculator,
        ISpaceAuthorizationCache authorizationCache)
    {
        _coreContext = coreContext;
        _institutionAuthorizationService = institutionAuthorizationService;
        _individualSpaceMemberCalculator = individualSpaceMemberCalculator;
        _authorizationCache = authorizationCache;
    }

    public async Task<AuthorizationResult> AuthorizeAsync(int spaceId, int userId,
        params SpacePermission[] requiredPermissions) =>
        ComputeAuthorizationResult(await GetAuthDataAsync(spaceId, userId), requiredPermissions);

    public async Task<ImmutableDictionary<int, AuthorizationResult>> AuthorizeManyAsync(IEnumerable<int> spaceIds,
        int userId,
        params SpacePermission[] requiredPermissions)
    {
        var allAuthData = await GetManyAuthDataAsync(spaceIds, userId);

        return allAuthData.ToImmutableDictionary(x => x.Key,
            x => ComputeAuthorizationResult(x.Value, requiredPermissions));
    }

    private static AuthorizationResult ComputeAuthorizationResult(SpaceUserAuthorizationInfo? authData,
        SpacePermission[] requiredPermissions)
    {
        if (authData is null)
        {
            return AuthorizationResult.HiddenForbidden;
        }

        var presentPermissions = GetPermissions(authData);
        var missingPermissions = requiredPermissions.Except(presentPermissions).ToArray();
        if (missingPermissions.Any())
        {
            // TODO: Show the missing permissions?
            return AuthorizationResult.AwareForbidden(new Error("You do not have permission to do this action.",
                "space.missingPermissions"));
        }

        return AuthorizationResult.Success;
    }

    private async Task<SpaceUserAuthorizationInfo?> GetAuthDataAsync(int spaceId, int userId)
    {
        var info = await _authorizationCache.GetAsync(spaceId, userId);
        if (info is not null)
        {
            return info;
        }

        info = await CalculateAuthDataAsync(spaceId, userId);
        if (info is not null)
        {
            await _authorizationCache.PutAsync(spaceId, info);
        }

        return info;
    }

    private async Task<Dictionary<int, SpaceUserAuthorizationInfo?>> GetManyAuthDataAsync(IEnumerable<int> spaceIds,
        int userId)
    {
        spaceIds = spaceIds.ToArray(); // Avoid multiple enumerations

        var cacheResults = await _authorizationCache.GetAllAsync(spaceIds, userId);
        var authDict = new Dictionary<int, SpaceUserAuthorizationInfo?>(cacheResults);
        List<(int, SpaceUserAuthorizationInfo)>? authToCache = null; // Create a list only if we need it.

        foreach (var spaceId in spaceIds)
        {
            if (authDict.GetValueOrDefault(spaceId) is null)
            {
                var authData = await CalculateAuthDataAsync(spaceId, userId);
                authDict[spaceId] = authData;

                if (authData is not null)
                {
                    authToCache ??= new List<(int, SpaceUserAuthorizationInfo)>();
                    authToCache.Add((spaceId, authData));
                }
            }
        }

        if (authToCache is not null)
        {
            await _authorizationCache.PutAllAsync(authToCache);
        }

        return authDict;
    }

    private async Task<SpaceUserAuthorizationInfo?> CalculateAuthDataAsync(int spaceId, int userId)
    {
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

    public async Task<OperationResult<IQueryable<Space>>> ApplyVisibilityFilter(IQueryable<Space> spaces,
        int institutionId, int userId)
    {
        spaces = spaces.Where(x => x.InstitutionId == institutionId);

        var authorizationResult = await _institutionAuthorizationService.AuthorizeAsync(institutionId, userId,
            InstitutionPermission.ManageAllSpaces);

        if (authorizationResult is HiddenForbiddenAuthorizationResult hiddenAuthorizationResult)
        {
            return OperationResult.Failure<IQueryable<Space>>(hiddenAuthorizationResult.Error);
        }

        var hasManageAllSpacesPermission = authorizationResult.IsSuccess;

        if (!hasManageAllSpacesPermission)
        {
            spaces = spaces.Where(x => x.Members.Any(m =>
                m is UserSpaceMember ? ((UserSpaceMember)m).UserId == userId :
                m is GroupSpaceMember ? ((GroupSpaceMember)m).Group.Users.Any(gu => gu.Id == userId) :
                false));
        }

        return OperationResult.Success(spaces);
    }
}