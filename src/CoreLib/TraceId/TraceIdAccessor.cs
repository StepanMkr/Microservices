using CoreLib.TraceId.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CoreLib.TraceId

public interface ITraceIdAccessor
{
    
}

public static class StartUpTraceId
{
    public static IServiceCollection TryAddTraceId(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<TraceIdAccessor>();
        serviceCollection
            .TryAddScoped<ITraceWriter>(provider => provider.GetRequiredService<TraceIdAccessor>());
        serviceCollection
            .TryAddScoped<ITraceReader>(provider => provider.GetRequiredService<TraceIdAccessor>());
        serviceCollection
            .TryAddScoped<ITraceIdAccessor>(provider => provider.GetRequiredService<TraceIdAccessor>());

        return serviceCollection;
    }
}

internal class TraceIdAccessor : ITraceReader, ITraceWriter, ITraceIdAccessor
{
    public string Name => "TraceId";

    private string _value;
    
    public string GetValue()
    {
        return _value ?? string.Empty;
    }

    public void WriteValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            value = Guid.NewGuid().ToString();
        }
        
        _value = value;
        LogContext.PushProperty("TraceId", value);
    }
}