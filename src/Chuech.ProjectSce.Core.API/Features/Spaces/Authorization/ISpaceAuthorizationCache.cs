using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Chuech.ProjectSce.Core.API.Features.Spaces.Authorization;

public interface ISpaceAuthorizationCache
{
    Task<SpaceUserAuthorizationInfo?> GetAsync(int spaceId, int userId);
    Task<IReadOnlyDictionary<int, SpaceUserAuthorizationInfo?>> GetAllAsync(IEnumerable<int> spaceIds, int userId);
    Task PutAsync(int spaceId, SpaceUserAuthorizationInfo info);
    Task PutAllAsync(IEnumerable<(int spaceId, SpaceUserAuthorizationInfo info)> entries);
    Task InvalidateAsync(int spaceId, params int[] userIds);
    Task InvalidateAllAsync(int spaceId);
}

public class RedisSpaceAuthorizationCache : ISpaceAuthorizationCache
{
    private static readonly TimeSpan s_ttl = TimeSpan.FromMinutes(5);

    private readonly IRedisDatabase _database;
    private readonly ILogger<RedisSpaceAuthorizationCache> _logger;
    private readonly JsonSerializerOptions _jsonSerializerOptions;

    public RedisSpaceAuthorizationCache(IRedisDatabase database,
        ILogger<RedisSpaceAuthorizationCache> logger,
        IOptions<JsonOptions> jsonSerializerOptions)
    {
        _database = database;
        _logger = logger;
        _jsonSerializerOptions = jsonSerializerOptions.Value.SerializerOptions;
    }

    public async Task<SpaceUserAuthorizationInfo?> GetAsync(int spaceId, int userId)
    {
        string infoJson = await _database.HashGetAsync(GetHashKey(spaceId), userId.ToString());

        return infoJson is not null
            ? JsonSerializer.Deserialize<SpaceUserAuthorizationInfo>(infoJson, _jsonSerializerOptions)
            : null;
    }

    public async Task<IReadOnlyDictionary<int, SpaceUserAuthorizationInfo?>> GetAllAsync(IEnumerable<int> spaceIds,
        int userId)
    {
        var tasks = spaceIds.ToDictionary(spaceId => spaceId, spaceId => GetAsync(spaceId, userId));
        await Task.WhenAll(tasks.Values);

        return tasks.ToDictionary(kv => kv.Key, kv => kv.Value.Result);
    }

    public async Task InvalidateAllAsync(int spaceId)
    {
        await _database.KeyDeleteAsync(GetHashKey(spaceId));
        _logger.LogDebug("Space authorization cache of all users have been cleared for space {SpaceId}", spaceId);
    }

    public async Task InvalidateAsync(int spaceId, params int[] userIds)
    {
        await _database.HashDeleteAsync(GetHashKey(spaceId), userIds.Select(x => (RedisValue)x).ToArray());
        _logger.LogDebug("Space authorization cache of users {@UserIds} has been cleared for space {SpaceId}",
            userIds, spaceId);
    }

    public async Task PutAsync(int spaceId, SpaceUserAuthorizationInfo info)
    {
        var key = GetHashKey(spaceId);

        await _database.HashSetAsync(key, new HashEntry[]
        {
            new(info.UserId.ToString(), JsonSerializer.Serialize(info, _jsonSerializerOptions))
        });
        await _database.KeyExpireAsync(key, s_ttl);

        _logger.LogDebug(
            "Space authorization cache of space {SpaceId} for user {UserId} has been updated: {@NewValue}",
            spaceId, info.UserId, info);
    }

    public Task PutAllAsync(IEnumerable<(int spaceId, SpaceUserAuthorizationInfo info)> entries)
        => Task.WhenAll(entries.Select(x => PutAsync(x.spaceId, x.info)));

    private static string GetHashKey(int spaceId)
    {
        return $"space:{spaceId}:auth";
    }
}