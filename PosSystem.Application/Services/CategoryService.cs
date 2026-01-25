using PosSystem.Application.DTOs.Category;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Application.Interfaces.Services;
using PosSystem.Domain.Entities;

namespace PosSystem.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;

    public CategoryService(ICategoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _repo.GetAllAsync();

        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            CategoryName = c.CategoryName,
            Description = c.Description
        }).ToList();
    }

    public async Task<CategoryDto> GetByIdAsync(Guid id)
    {
        var category = await _repo.GetByIdAsync(id)
            ?? throw new Exception("Category not found");

        return new CategoryDto
        {
            Id = category.Id,
            CategoryName = category.CategoryName,
            Description = category.Description
        };
    }

    public async Task CreateAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            CategoryName = dto.CategoryName,
            Description = dto.Description ?? string.Empty
        };

        await _repo.AddAsync(category);
    }

    public async Task UpdateAsync(Guid id, UpdateCategoryDto dto)
    {
        var category = await _repo.GetByIdAsync(id)
            ?? throw new Exception("Category not found");

        category.CategoryName = dto.CategoryName;
        category.Description = dto.Description ?? string.Empty;

        await _repo.UpdateAsync(category);
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await _repo.GetByIdAsync(id)
            ?? throw new Exception("Category not found");

        await _repo.DeleteAsync(category);
    }
}