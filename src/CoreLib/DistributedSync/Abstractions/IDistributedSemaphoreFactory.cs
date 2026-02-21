namespace CoreLib.DistributedSync.Abstractions;

public interface IDistributedSemaphoreFactory
{
    IDistributedSemaphore Create(string name, int maxCount, TimeSpan lease);
}