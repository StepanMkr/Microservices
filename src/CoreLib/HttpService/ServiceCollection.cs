using CoreLib.HttpService.Services;
using CoreLib.HttpService.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CoreLib.HttpService;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHttpRequestService(this IServiceCollection services)
    {
        services.AddHttpClient();

        services.TryAddTransient<IHttpConnectionService, HttpConnectionService>();
        services.TryAddTransient<IHttpRequestService, HttpRequestService>();

        return services;
    }
}