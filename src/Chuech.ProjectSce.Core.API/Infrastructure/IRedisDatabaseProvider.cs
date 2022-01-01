using StackExchange.Redis;

namespace Chuech.ProjectSce.Core.API.Infrastructure;

public interface IRedisDatabaseProvider
{
    IRedisDatabase GetDatabase();
    IConnectionMultiplexer GetMultiplexer();
}

public class RedisDatabaseProvider : IRedisDatabaseProvider
{
    private readonly Lazy<IConnectionMultiplexer> _lazyMultiplexer;

    public RedisDatabaseProvider(IConfiguration configuration)
    {
        _lazyMultiplexer = new(() => ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis"),
            configure =>
            {
                // Make the multiplexer silently retry in case of connection issues
                configure.AbortOnConnectFail = false;
                configure.ReconnectRetryPolicy = new ExponentialRetry(2000, 20000);
                configure.SyncTimeout = 3000;
            }));
    }

    public IRedisDatabase GetDatabase()
    {
        return _lazyMultiplexer.Value.GetDatabase();
    }

    public IConnectionMultiplexer GetMultiplexer()
    {
        return _lazyMultiplexer.Value;
    }
}