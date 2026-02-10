using Microsoft.EntityFrameworkCore;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Domain.Entities;
using PosSystem.Infrastructure.Persistence;

namespace PosSystem.Infrastructure.Persistence.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AppDbContext _dbContext;

        public CustomerRepository(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Customer>> GetAllAsync()
        {
            return await _dbContext.Customers.ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(Guid id)
        {
            return await _dbContext.Customers.FindAsync(id);
        }

        public async Task AddAsync(Customer entity)
        {
            await _dbContext.Customers.AddAsync(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAsync(Customer entity)
        {
            _dbContext.Customers.Update(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Customer entity)
        {
            _dbContext.Customers.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }
    }
}