using CoreLib.DistributedSync.Abstractions;
using StackExchange.Redis;

namespace CoreLib.DistributedSync.Redis;

public sealed class RedisDistributedSemaphoreHandle : IDistributedSemaphoreHandle
{
    private readonly IDatabase _db;
    private readonly RedisKey _key;
    private readonly string _lockId;
    private readonly TimeSpan _lease;
    private readonly TimeSpan _extensionCadence;
    private CancellationTokenSource? _cts;
    private Task? _loop;

    private const string ExtendScriptText = @"
redis.replicate_commands()
local nowResult = redis.call('TIME')
local nowMillis = (tonumber(nowResult[1]) * 1000) + math.floor(tonumber(nowResult[2]) / 1000)
-- extend only if exists (XX)
local result = redis.call('ZADD', KEYS[1], 'XX', 'CH', nowMillis + tonumber(ARGV[1]), ARGV[2])
local keyTtl = redis.call('PTTL', KEYS[1])
if keyTtl < tonumber(ARGV[3]) then
    redis.call('PEXPIRE', KEYS[1], tonumber(ARGV[3]))
end
return result
";

    public RedisDistributedSemaphoreHandle(IDatabase db, RedisKey key, string lockId, TimeSpan lease, TimeSpan extensionCadence)
    {
        _db = db;
        _key = key; 
        _lockId = lockId; 
        _lease = lease;
        _extensionCadence = extensionCadence;
    }

    public void StartExtensionLoop()
    {
        _cts = new CancellationTokenSource();
        _loop = Task.Run(async () =>
        {
            var setTtl = TimeSpan.FromMilliseconds(Math.Min(int.MaxValue, _lease.TotalMilliseconds * 3));
            while (!_cts!.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(_extensionCadence, _cts.Token).ConfigureAwait(false);

                    await _db.ScriptEvaluateAsync(ExtendScriptText, [_key], [(int)_lease.TotalMilliseconds, _lockId, (int)setTtl.TotalMilliseconds]).ConfigureAwait(false);
                }
                catch (OperationCanceledException) { }
            }
        });
    }

    public async ValueTask DisposeAsync()
    {
        try { await _cts?.CancelAsync()!; } catch { }
        try { if (_loop is not null) await _loop.ConfigureAwait(false); } catch { }
        try { await _db.SortedSetRemoveAsync(_key, _lockId, CommandFlags.DemandMaster).ConfigureAwait(false); } catch { }
        _cts?.Dispose();
    }
}