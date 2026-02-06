
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PosSystem.Application.DTOs.Customer;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Application.Interfaces.Services;
using PosSystem.Application.Services;
using PosSystem.Domain.Entities;

namespace PosSystem.Api.Controllers;

[ApiController]
[Route("api/controller")]
public class CustomerController :ControllerBase
{
    private readonly ICustomerService _service;

    public CustomerController(ICustomerService service)
    {
        _service = service ; 
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok("All Customeries" + await _service.GetAllAdsync());
    }
    [HttpGet("id")]
    public async Task<ActionResult> GetById(Guid id)
    {
        return Ok(await _service.GetByIdAsync(id));
    }
    [HttpPost]
    [Authorize(Policy = "CanCreateCustomer")]
    public async Task<ActionResult> Create(CreateCustomerDto dto)
    {
         await _service.CreateAsync(dto);
         return Ok("Customer create successfully");
    }
    [HttpPut("id")]
    [Authorize(Policy = "CanUpdateCustomer")]
    public async Task<ActionResult> Update(Guid id , UpdateCustomerDto dto)
    {
        await _service.UpdateAsync(id, dto);
        return Ok("Updated customer successfully");
    }
    [HttpDelete]
    [Authorize(Policy = "CanDeleteCustomer")]
    public async Task<ActionResult> Delete (Guid id)
    {
        await _service.DeleteAsync(id);
        return Ok("Delete customer successfully");
    }
} 
