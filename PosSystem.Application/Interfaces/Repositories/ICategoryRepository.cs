using PosSystem.Domain.Entities;
using System;

namespace PosSystem.Application.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(Guid id);
    Task AddAsync(Category entity);
    Task UpdateAsync(Category entity);
    Task DeleteAsync(Category entity);
}