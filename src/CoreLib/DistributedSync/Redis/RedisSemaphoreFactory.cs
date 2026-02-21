using CoreLib.DistributedSync.Abstractions;
using StackExchange.Redis;

namespace CoreLib.DistributedSync.Redis;

public sealed class RedisSemaphoreFactory : IDistributedSemaphoreFactory
{
    private readonly IConnectionMultiplexer _muxer;
    private readonly int _dbNumber;

    public RedisSemaphoreFactory(IConnectionMultiplexer muxer, int dbNumber = -1)
    {
        _muxer = muxer;
        _dbNumber = dbNumber;
    }

    public IDistributedSemaphore Create(string name, int maxCount, TimeSpan lease)
    {
        var database = _muxer.GetDatabase(_dbNumber);
        
        return new RedisDistributedSemaphore(database, name, maxCount, lease);
    }
}