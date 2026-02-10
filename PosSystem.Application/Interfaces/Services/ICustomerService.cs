

using PosSystem.Application.DTOs.Customer;

namespace PosSystem.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetAllAsync();
    Task<CustomerDto> GetByIdAsync(Guid id);
    Task CreateAsync(CreateCustomerDto dto);
    Task UpdateAsync(Guid id, UpdateCustomerDto dto);
    Task DeleteAsync(Guid id);
}

