using Polly;
using Polly.Registry;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;

public class ResilientInstitutionAuthorizationCache : IInstitutionAuthorizationCache
{
    private readonly IInstitutionAuthorizationCache _cache;
    private readonly ChuechPolicyRegistry _policyRegistry;

    public ResilientInstitutionAuthorizationCache(IInstitutionAuthorizationCache cache,
        ChuechPolicyRegistry policyRegistry)
    {
        _cache = cache;
        _policyRegistry = policyRegistry;
    }

    private IAsyncPolicy Policy => _policyRegistry.DistributedCachePolicy;

    public Task<InstitutionAuthorizationThumbprint?> GetAsync(int institutionId, int userId)
        => Policy.ExecuteAsync(_ => _cache.GetAsync(institutionId, userId), 
            new Context("institution-auth-cache:get"));

    public Task SetAsync(InstitutionAuthorizationThumbprint thumbprint)
        => Policy.ExecuteAsync(_ => _cache.SetAsync(thumbprint),
            new Context("institution-auth-cache:set"));

    public Task InvalidateAsync(int institutionId, int userId)
        => Policy.ExecuteAsync(_ => _cache.InvalidateAsync(institutionId, userId),
            new Context("institution-auth-cache:invalidate"));
}