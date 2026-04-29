using Microsoft.Extensions.DependencyInjection;
using LuckyMaze.Infrastructure.Services;

namespace LuckyMaze.Infrastructure.BackgroundServices
{
    public static class HostedServicesRegistration
    {
        public static IServiceCollection AddHostedServices(this IServiceCollection services)
        {
            services.AddHostedService<ModelSyncService>();
            return services;
        }
    }
}
