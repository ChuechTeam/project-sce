using Polly;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Authorization;

public class ResilientSpaceAuthorizationCache : ISpaceAuthorizationCache
{
    private readonly ISpaceAuthorizationCache _cache;
    private readonly ChuechPolicyRegistry _registry;

    public ResilientSpaceAuthorizationCache(ISpaceAuthorizationCache cache, ChuechPolicyRegistry registry)
    {
        _cache = cache;
        _registry = registry;
    }

    private IAsyncPolicy Policy => _registry.DistributedCachePolicy;

    public Task<SpaceUserAuthorizationInfo?> GetAsync(int spaceId, int userId)
        => Policy.ExecuteAsync(_ => _cache.GetAsync(spaceId, userId), 
            new Context("space-auth-cache:get"));

    public Task<IReadOnlyDictionary<int, SpaceUserAuthorizationInfo?>> GetAllAsync(IEnumerable<int> spaceIds,
        int userId)
        => Policy.ExecuteAsync(_ => _cache.GetAllAsync(spaceIds, userId), 
            new Context("space-auth-cache:get-all"));

    public Task PutAsync(int spaceId, SpaceUserAuthorizationInfo info)
        => Policy.ExecuteAsync(_ => _cache.PutAsync(spaceId, info), 
            new Context("space-auth-cache:put"));

    public Task PutAllAsync(IEnumerable<(int spaceId, SpaceUserAuthorizationInfo info)> entries)
        => Policy.ExecuteAsync(_ => _cache.PutAllAsync(entries), 
            new Context("space-auth-cache:put-all"));

    public Task InvalidateAsync(int spaceId, params int[] userIds)
        => Policy.ExecuteAsync(_ => _cache.InvalidateAsync(spaceId, userIds), 
            new Context("space-auth-cache:invalidate"));

    public Task InvalidateAllAsync(int spaceId)
        => Policy.ExecuteAsync(_ => _cache.InvalidateAllAsync(spaceId), 
            new Context("space-auth-cache:invalidate-all"));
}