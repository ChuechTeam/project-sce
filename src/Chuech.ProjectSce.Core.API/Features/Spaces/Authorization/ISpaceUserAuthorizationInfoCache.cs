using System.Diagnostics;
using System.Text.Json;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Authorization;

public interface ISpaceUserAuthorizationInfoCache
{
    Task<SpaceUserAuthorizationInfo?> GetAsync(int spaceId, int userId);
    Task PutAsync(int spaceId, SpaceUserAuthorizationInfo info, TimeSpan ttl);
    Task<bool> InvalidateAsync(int spaceId, int userId);
    Task<bool> InvalidateAllAsync(int spaceId);
}
public class RedisSpaceUserAuthorizationInfoCache : ISpaceUserAuthorizationInfoCache
{
    private readonly IRedisDatabase _database;
    private readonly ILogger<RedisSpaceUserAuthorizationInfoCache> _logger;

    public RedisSpaceUserAuthorizationInfoCache(IRedisDatabase database, ILogger<RedisSpaceUserAuthorizationInfoCache> logger)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<SpaceUserAuthorizationInfo?> GetAsync(int spaceId, int userId)
    {
        string infoJson = await _database.HashGetAsync(GetHashKey(spaceId), userId.ToString());

        if (infoJson is null)
        {
            return null;
        }

        return JsonSerializer.Deserialize<SpaceUserAuthorizationInfo>(infoJson);
    }

    public async Task<bool> InvalidateAllAsync(int spaceId)
    {
        return await _database.KeyDeleteAsync(GetHashKey(spaceId));
    }

    public async Task<bool> InvalidateAsync(int spaceId, int userId)
    {
        return await _database.HashDeleteAsync(GetHashKey(spaceId), userId.ToString());
    }

    public async Task PutAsync(int spaceId, SpaceUserAuthorizationInfo info, TimeSpan ttl)
    {
        var infoJson = JsonSerializer.Serialize(info);
        var key = GetHashKey(spaceId);

        await _database.HashSetAsync(key, new StackExchange.Redis.HashEntry[] {
            new(info.UserId.ToString(), infoJson)
        });
        await _database.KeyExpireAsync(key, ttl);
    }

    private static string GetHashKey(int spaceId)
    {
        return $"space:{spaceId}:users-auth";
    }
}