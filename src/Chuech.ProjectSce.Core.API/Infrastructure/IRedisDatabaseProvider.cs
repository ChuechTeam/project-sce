using Microsoft.EntityFrameworkCore.Storage;
using StackExchange.Redis;

namespace Chuech.ProjectSce.Core.API.Infrastructure;

public interface IRedisDatabaseProvider
{
    IRedisDatabase GetDatabase();
}
public class RedisDatabaseProvider : IRedisDatabaseProvider
{
    private readonly Lazy<IConnectionMultiplexer> _lazyMultiplexer;
    public RedisDatabaseProvider(IConfiguration configuration)
    {
        _lazyMultiplexer = new(() => ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")));
    }

    public IRedisDatabase GetDatabase()
    {
        return _lazyMultiplexer.Value.GetDatabase();
    }
}
