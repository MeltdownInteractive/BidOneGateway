namespace BidOneGateway.Domain.Models.Dto;

public class ProductDto {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int StockLevel { get; set; } 
    public string? NewV2Field { get; set; }
}