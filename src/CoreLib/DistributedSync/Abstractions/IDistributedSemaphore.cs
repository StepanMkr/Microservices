namespace CoreLib.DistributedSync.Abstractions;

public interface IDistributedSemaphore
{
    string Name { get; }
    
    int MaxCount { get; }
    
    ValueTask<IDistributedSemaphoreHandle?> TryAcquireAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
    
    ValueTask<IDistributedSemaphoreHandle> AcquireAsync(TimeSpan timeout, CancellationToken cancellationToken = default);
    
    ValueTask<int> GetCurrentCountAsync(CancellationToken cancellationToken = default);
}