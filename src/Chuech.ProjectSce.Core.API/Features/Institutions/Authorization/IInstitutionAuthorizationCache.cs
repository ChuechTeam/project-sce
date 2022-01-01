using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Chuech.ProjectSce.Core.API.Features.Institutions.Authorization;

public interface IInstitutionAuthorizationCache
{
    Task<InstitutionAuthorizationThumbprint?> GetAsync(int institutionId, int userId);
    Task SetAsync(InstitutionAuthorizationThumbprint thumbprint);
    Task InvalidateAsync(int institutionId, int userId);
}

public class RedisInstitutionAuthorizationCache
    : IInstitutionAuthorizationCache
{
    private static readonly Duration s_ttl = Duration.FromMinutes(5);

    private readonly IDatabase _database;
    private readonly IClock _clock;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly ILogger<RedisInstitutionAuthorizationCache> _logger;

    public RedisInstitutionAuthorizationCache(IOptions<JsonOptions> jsonOptions, IRedisDatabase database, IClock clock,
        ILogger<RedisInstitutionAuthorizationCache> logger)
    {
        _database = database;
        _clock = clock;
        _logger = logger;
        _jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
    }

    private static (RedisKey hashKey, RedisValue fieldKey, RedisValue fieldExpKey) GetKeys(int institutionId,
        int userId)
    {
        return ($"institution:{institutionId}:auth", userId.ToString(), userId + "-exp");
    }

    public async Task<InstitutionAuthorizationThumbprint?> GetAsync(int institutionId, int userId)
    {
        var (hashKey, fieldKey, fieldExpKey) = GetKeys(institutionId, userId);

        var values = await _database.HashGetAsync(hashKey, new[] { fieldKey, fieldExpKey });
        if (values.Length != 2)
        {
            return null;
        }
        
        var (json, exp) = (values[0], RedisToInstant(values[1]));
        if (exp is null || exp < _clock.GetCurrentInstant())
        {
            _logger.LogDebug(
                "Institution authorization cache expired for institution {InstitutionId} and user {UserId}",
                institutionId, userId);
            _database.HashDelete(hashKey, new[] { fieldKey, fieldExpKey }, CommandFlags.FireAndForget);
            return null;
        }

        return JsonSerializer.Deserialize<InstitutionAuthorizationThumbprint>(json, _jsonSerializerOptions);
    }

    public async Task SetAsync(InstitutionAuthorizationThumbprint thumbprint)
    {
        var (hashKey, fieldKey, fieldExpKey) = GetKeys(thumbprint.InstitutionId, thumbprint.UserId);
        var json = JsonSerializer.Serialize(thumbprint, _jsonSerializerOptions);

        await _database.HashSetAsync(hashKey, new HashEntry[]
        {
            new(fieldKey, json),
            new(fieldExpKey, _clock.GetCurrentInstant().Plus(s_ttl).ToUnixTimeMilliseconds())
        });
        
        _logger.LogDebug(
            "Institution authorization cache has been updated for institution {InstitutionId} " +
            "and user {UserId}: {@NewValue}",
            thumbprint.InstitutionId, thumbprint.UserId, thumbprint);
    }

    public async Task InvalidateAsync(int institutionId, int userId)
    {
        var (hashKey, fieldKey, fieldExpKey) = GetKeys(institutionId, userId);
        await _database.HashDeleteAsync(hashKey, new[] { fieldKey, fieldExpKey });
        
        _logger.LogDebug(
            "Institution authorization cache has been cleared for institution {InstitutionId} " +
            "and user {UserId}",
            institutionId, userId);
    }

    private static Instant? RedisToInstant(RedisValue value)
    {
        if (long.TryParse(value, out var milliseconds))
        {
            return Instant.FromUnixTimeMilliseconds(milliseconds);
        }
        else
        {
            return null;
        }
    }
}