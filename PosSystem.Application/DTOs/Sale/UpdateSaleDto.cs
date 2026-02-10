using System.ComponentModel.DataAnnotations;

namespace PosSystem.Application.DTOs.Sale;

public class UpdateSaleDto
{
    public Guid CustomerId { get; set; }
    public List<CreateSaleItemDto> SaleItems { get; set; } = new();
}