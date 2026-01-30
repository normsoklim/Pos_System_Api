using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Domain.Entities;
using BCrypt.Net;

namespace PosSystem.Api.Services;

public static class SeedService
{
    public static async Task SeedDefaultRoles(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleRepository = scope.ServiceProvider.GetRequiredService<IRoleRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        
        // Create default roles
        var roles = new List<string> { "Admin", "Manager", "Employee", "User" };

        foreach (var roleName in roles)
        {
            var existingRole = await roleRepository.GetByNameAsync(roleName);
            if (existingRole == null)
            {
                var role = new Role
                {
                    Name = roleName,
                    Description = $"{roleName} role with default permissions",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };
                await roleRepository.CreateAsync(role);
            }
        }

        // Create default admin user if not exists
        var adminRole = await roleRepository.GetByNameAsync("Admin");
        var adminUser = await userRepository.GetByUsernameAsync("admin");
        if (adminUser == null && adminRole != null)
        {
            var newAdminUser = new User
            {
                Username = "admin",
                Email = "admin@pos-system.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                FirstName = "System",
                LastName = "Administrator",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdAdmin = await userRepository.CreateAsync(newAdminUser);
            await userRepository.AssignRoleToUserAsync(createdAdmin.Id, adminRole.Id);
        }
    }
}