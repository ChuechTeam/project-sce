using System.Net.Sockets;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using StackExchange.Redis;

namespace Chuech.ProjectSce.Core.API.Infrastructure.Resilience;

/// <summary>
/// Contains various Polly policies that can be updated at runtime using options. 
/// </summary>
public class ChuechPolicyRegistry : PolicyRegistry
{
    private readonly ILogger<ChuechPolicyRegistry> _logger;

    private const string OptimisticConcurrencyKey = "OptimisticConcurrency";
    private const string DistributedCacheKey = "DistributedCache";
    private const string CacheFailureIgnoreKey = "CacheFailureIgnore";

    public ChuechPolicyRegistry(ILogger<ChuechPolicyRegistry> logger)
    {
        _logger = logger;

        SetOptimisticConcurrencyPolicy();
        SetDistributedCachePolicy();
        SetCacheFailureIgnorePolicy();
    }

    #region OptimisticConcurrencyPolicy

    private void SetOptimisticConcurrencyPolicy()
    {
        this[OptimisticConcurrencyKey] =
            Policy.Handle<DbUpdateConcurrencyException>()
                .RetryAsync(3, onRetry: (_, current, context) =>
                {
                    _logger.LogTrace("Optimistic concurrency failure for " +
                                     "operation {OperationKey} (retry #{RetryCount})",
                        context.OperationKey, current);
                });
    }

    public IAsyncPolicy OptimisticConcurrencyPolicy => Get<IAsyncPolicy>(OptimisticConcurrencyKey);

    #endregion

    #region DistributedCachePolicy

    private void SetDistributedCachePolicy()
    {
        this[DistributedCacheKey] = Policy.Handle<RedisConnectionException>()
            .Or<RedisTimeoutException>()
            .Or<SocketException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 10,
                durationOfBreak: TimeSpan.FromSeconds(15),
                onBreak: (exception, duration, context) =>
                {
                    _logger.LogWarning(exception,
                        "Distributed cache circuit breaker is broken for {BreakSeconds} " +
                        "seconds in operation {OperationKey}", duration.Seconds, context.OperationKey);
                },
                onHalfOpen: () => _logger.LogInformation("Distributed cache circuit breaker is half-open"),
                onReset: context =>
                {
                    _logger.LogInformation(
                        "Distributed cache circuit break has been reset in operation {OperationKey}",
                        context.OperationKey);
                });
    }

    public IAsyncPolicy DistributedCachePolicy => Get<IAsyncPolicy>(DistributedCacheKey);

    #endregion

    #region CacheFailureIgnorePolicy

    public IAsyncPolicy<T?> GetCacheFailureIgnorePolicy<T>() where T : class
    {
        return Policy<T?>.Handle<Exception>()
            .FallbackAsync((T?)null, (result, context) => LogCacheFailure(result.Exception, context));
    }

    private void SetCacheFailureIgnorePolicy()
    {
        this[CacheFailureIgnoreKey] = Policy.Handle<Exception>()
            .FallbackAsync((_, _) => Task.CompletedTask, onFallbackAsync: LogCacheFailure);
    }

    public IAsyncPolicy CacheFailureIgnorePolicy => Get<IAsyncPolicy>(CacheFailureIgnoreKey);

    private Task LogCacheFailure(Exception exception, Context context)
    {
        if (exception is not BrokenCircuitException)
        {
            _logger.LogWarning(exception, "Cache usage failed for operation {OperationKey}", context.OperationKey);
        }
        else
        {
            _logger.LogDebug("Cache usage avoided due to a broken circuit in operation {OperationKey}",
                context.OperationKey);
        }

        return Task.CompletedTask;
    }

    #endregion
}