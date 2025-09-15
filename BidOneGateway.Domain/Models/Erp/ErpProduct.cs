namespace BidOneGateway.Domain.Models.Erp;

public class ErpProduct
{
    public int Id { get; set; } 
    public required string Sku { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
}