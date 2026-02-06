
using PosSystem.Domain.Entities;

namespace PosSystem.Application.Interfaces.Repositories;


public interface ICustomerRepository
{
    Task<List<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(Guid id);
    Task AddAsync(Customer entity);
    Task UpdateAsync(Customer entity);
    Task DeleteAsync(Customer entity);
}