using Microsoft.Extensions.DependencyInjection;
using TaskService.Domain.Interfaces;
using TaskService.Infrastructure.Persistence;

namespace TaskService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IProjectRepository, ProjectRepository>();
        return services;
    }
}