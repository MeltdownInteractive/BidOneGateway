namespace BidOneGateway.Domain.Models.Warehouse;

public class WarehouseStock
{
    public int ProductId { get; set; } 
    public int StockLevel { get; set; }
    public required string WarehouseLocation { get; set; }
}