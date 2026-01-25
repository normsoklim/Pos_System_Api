
using Microsoft.AspNetCore.Mvc;
using PosSystem.Application.DTOs.Category;
using PosSystem.Application.Interfaces.Services;
using System;

namespace PosSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoryController(ICategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCategoryDto dto)
    {
        await _service.CreateAsync(dto);
        return Ok("Category created");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return Ok("Category updated");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok("Category deleted");
    }
}