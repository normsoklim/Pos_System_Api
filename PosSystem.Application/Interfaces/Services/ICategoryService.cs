using PosSystem.Application.DTOs.Category;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PosSystem.Application.Interfaces.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync();
    Task<CategoryDto> GetByIdAsync(Guid id);
    Task CreateAsync(CreateCategoryDto dto);
    Task UpdateAsync(Guid id, UpdateCategoryDto dto);
    Task DeleteAsync(Guid id);
}