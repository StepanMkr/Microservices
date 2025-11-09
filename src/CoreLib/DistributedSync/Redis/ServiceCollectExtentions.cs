using CoreLib.DistributedSync.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CoreLib.DistributedSync.Redis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisDistributedSync(this IServiceCollection services, IConfiguration cfg)
    {
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var connectionString = cfg.GetConnectionString("Redis")
                                   ?? Environment.GetEnvironmentVariable("REDIS")
                                   ?? "localhost:6379";
            return ConnectionMultiplexer.Connect(connectionString);
        });
        services.AddSingleton<IDistributedSemaphoreFactory, RedisSemaphoreFactory>();
        return services;
    }
}