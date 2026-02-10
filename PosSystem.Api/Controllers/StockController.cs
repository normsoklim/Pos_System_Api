

using Microsoft.AspNetCore.Mvc;
using PosSystem.Application.DTOs.Stock;
using PosSystem.Application.interfaces.Services;
using PosSystem.Domain.Entities;

namespace PosSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
    private readonly IStockService _service; 

    public StockController(IStockService service)
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
    public async Task<IActionResult> Create(CreateStockDto dto)
    {
        await _service.CreateAsync(dto);
        return Ok(new { message = "Stock created successfully" });
    }

     [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateStockDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return Ok(new { message = "Stock updated successfully" });
    }

     [HttpDelete("{id}")]   
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok(new { message = "Stock deleted successfully" });
    }
}
