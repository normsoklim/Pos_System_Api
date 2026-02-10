
using PosSystem.Application.DTOs.Customer;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Application.Interfaces.Services;
using PosSystem.Domain.Entities;

namespace PosSystem.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repo;

    public CustomerService(ICustomerRepository repo)
    {
        _repo = repo;
    }


    public async Task<List<CustomerDto>> GetAllAsync()
    {
        var customers = await _repo.GetAllAsync();
        return customers.Select(c => new CustomerDto
        {
            Id = c.Id,
            CustomerName = c.CustomerName,
            Email = c.Email,
            Phone = c.Phone,
            Address = c.Address
        }).ToList();
    }
    public async Task<CustomerDto> GetByIdAsync(Guid id)
    {
        var customer = await _repo.GetByIdAsync(id) 
        ?? throw new Exception("Customer not found");
        return new CustomerDto
        {
           Id = customer.Id,
           CustomerName = customer.CustomerName,
           Email = customer.Email,
           Phone = customer.Phone,
           Address = customer.Address
        };
    }

    public async Task CreateAsync(CreateCustomerDto dto)
    {
       var customer = new Customer
       {
          Id = Guid.NewGuid(),
          CustomerName = dto.CustomerName,
          Email = dto.Email,
          Phone = dto.Phone,
          Address = dto.Address

       };
       await _repo.AddAsync(customer);
    }

    public async Task UpdateAsync(Guid id, UpdateCustomerDto dto)
    {
       var customer = await _repo.GetByIdAsync(id)
       ?? throw new Exception ("Customer not found");

       customer.CustomerName = dto.CustomerName;
       customer.Email = dto.Email;
       customer.Phone = dto.Phone;
       customer.Address = dto.Address;

       await _repo.UpdateAsync(customer);

    }

    public async Task DeleteAsync(Guid id)
    {
       var customer = await _repo.GetByIdAsync(id)
       ?? throw new Exception ("Customer not found");

       await _repo.DeleteAsync(customer);
    }
}