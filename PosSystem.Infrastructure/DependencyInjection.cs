using Microsoft.Extensions.DependencyInjection;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Application.Interfaces.Services;
using PosSystem.Application.Services;
using PosSystem.Infrastructure.Persistence.Repositories;
using PosSystem.Application.Security;


namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICategoryService, CategoryService>();
            
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductService, ProductService>();
            
            // Authentication services
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<ISecurityAuditService, SecurityAuditService>();
            
            // Add other service registrations here as needed
            
            return services;
        }
    }
}
