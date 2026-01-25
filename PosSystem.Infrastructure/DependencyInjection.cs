
using Microsoft.Extensions.DependencyInjection;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Application.Interfaces.Services;
using PosSystem.Application.Services;
using PosSystem.Infrastructure.Persistence.Repositories;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICategoryService, CategoryService>();
            
            // Add other service registrations here as needed
            
            return services;
        }
    }
}
