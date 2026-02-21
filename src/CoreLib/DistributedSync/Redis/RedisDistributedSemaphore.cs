using CoreLib.DistributedSync.Abstractions;
using StackExchange.Redis;

namespace CoreLib.DistributedSync.Redis;

public sealed class RedisDistributedSemaphore : IDistributedSemaphore
{
    private readonly IDatabase _db;
    private readonly RedisKey _key;
    private readonly int _maxCount;
    private readonly TimeSpan _lease;
    private readonly TimeSpan _extensionCadence;

    private const string AcquireScriptText = @"
redis.replicate_commands()
local nowResult = redis.call('TIME')
local nowMillis = (tonumber(nowResult[1]) * 1000) + math.floor(tonumber(nowResult[2]) / 1000)
-- purge expired
redis.call('ZREMRANGEBYSCORE', KEYS[1], '-inf', nowMillis)
-- space left?
local count = redis.call('ZCARD', KEYS[1])
if count < tonumber(ARGV[1]) then
    redis.call('ZADD', KEYS[1], nowMillis + tonumber(ARGV[2]), ARGV[3])
    -- renew set TTL to 3x lease if needed
    local keyTtl = redis.call('PTTL', KEYS[1])
    if keyTtl < tonumber(ARGV[4]) then
        redis.call('PEXPIRE', KEYS[1], tonumber(ARGV[4]))
    end
    return 1
end
return 0
";

    private const string PurgeAndCountScriptText = @"
redis.replicate_commands()
local nowResult = redis.call('TIME')
local nowMillis = (tonumber(nowResult[1]) * 1000) + math.floor(tonumber(nowResult[2]) / 1000)
redis.call('ZREMRANGEBYSCORE', KEYS[1], '-inf', nowMillis)
return redis.call('ZCARD', KEYS[1])
";

    public RedisDistributedSemaphore(IDatabase db, string name, int maxCount, TimeSpan lease)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _key = name ?? throw new ArgumentNullException(nameof(name));
        if (maxCount < 1)
            throw new ArgumentOutOfRangeException(nameof(maxCount), "Maximum count must be at least 1.");

        _maxCount = maxCount;
        _lease = lease > TimeSpan.Zero ? lease : TimeSpan.FromSeconds(30);
        _extensionCadence = TimeSpan.FromMilliseconds(_lease.TotalMilliseconds / 3d);
    }

    public string Name => _key.ToString();
    public int MaxCount => _maxCount;

    public async ValueTask<int> GetCurrentCountAsync(CancellationToken ct = default)
    {
        var result = await _db.ScriptEvaluateAsync(
            PurgeAndCountScriptText,
            new RedisKey[] { _key }
        ).ConfigureAwait(false);

        var used = (int)(long)result;
        var available = _maxCount - used;
        return available < 0 ? 0 : available;
    }

    public async ValueTask<IDistributedSemaphoreHandle?> TryAcquireAsync(TimeSpan timeout, CancellationToken ct = default)
    {
        var deadline = DateTime.UtcNow + timeout;
        var setTtl = TimeSpan.FromMilliseconds(Math.Min(int.MaxValue, _lease.TotalMilliseconds * 3));
        var lockId = Guid.NewGuid().ToString("N");

        while (!ct.IsCancellationRequested)
        {
            var result = await _db.ScriptEvaluateAsync(
                AcquireScriptText,
                new RedisKey[] { _key },
                new RedisValue[] { _maxCount, (int)_lease.TotalMilliseconds, lockId, (int)setTtl.TotalMilliseconds }
            ).ConfigureAwait(false);

            var acquired = (int)(long)result;
            if (acquired == 1)
            {
                var handle = new RedisDistributedSemaphoreHandle(_db, _key, lockId, _lease, _extensionCadence);
                handle.StartExtensionLoop();
                return handle;
            }

            if (DateTime.UtcNow >= deadline)
                return null;

            try
            {
                await Task.Delay(Random.Shared.Next(10, 40), ct).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                throw new TimeoutException("Semaphore acquisition canceled or timed out.");
            }
        }

        return null;
    }

    public async ValueTask<IDistributedSemaphoreHandle> AcquireAsync(TimeSpan timeout, CancellationToken ct = default)
    {
        var handle = await TryAcquireAsync(timeout, ct).ConfigureAwait(false);
        if (handle is null)
            throw new TimeoutException($"Semaphore '{Name}' acquire timed out after {timeout.TotalSeconds:F1}s.");

        return handle;
    }
}
