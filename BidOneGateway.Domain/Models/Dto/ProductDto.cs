namespace BidOneGateway.Domain.Models.Dto;

public class ProductDto
{
    public int Id { get; set; }
    public required string Sku { get; set; }
    public required string Name { get; set; }
    public int StockLevel { get; set; }
}