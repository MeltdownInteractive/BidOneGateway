using BidOneGateway.Domain.Models.Warehouse;

namespace BidOneGateway.Application.Interfaces;

public interface IWarehouseClient
{
    Task<WarehouseStock?> GetStockForProductAsync(int productId);
}