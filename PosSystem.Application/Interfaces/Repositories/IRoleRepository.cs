using PosSystem.Domain.Entities;

namespace PosSystem.Application.Interfaces.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string name);
    Task<Role?> GetByIdAsync(int id);
    Task<List<Role>> GetAllAsync();
    Task<Role> CreateAsync(Role role);
    Task<bool> UpdateAsync(Role role);
    Task<bool> DeleteAsync(int id);
}