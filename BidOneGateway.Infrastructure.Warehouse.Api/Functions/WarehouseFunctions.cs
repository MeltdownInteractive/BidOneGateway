using BidOneGateway.Domain.Models.Warehouse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BidOneGateway.Infrastructure.Warehouse.Api.Functions;

public class WarehouseFunctions(ILogger<WarehouseFunctions> logger)
{
    private static readonly List<WarehouseStock> StockLevels =
    [
        new WarehouseStock
        {
            ProductId = 1,
            StockLevel = 150,
            WarehouseLocation = "Aisle 12, Bay 4"
        },

        new WarehouseStock
        {
            ProductId = 2,
            StockLevel = 0,
            WarehouseLocation = "Aisle 3, Bay 1"
        }
    ];

    [Function("WarehouseGetStock")]
    public IActionResult GetStockByProductId(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "warehouse/stock/{productId:int}")] HttpRequest req, int productId)
    {
        logger.LogInformation("STUB WAREHOUSE: Received request for stock of product {ProductId}", productId);
        
        var stock = StockLevels.FirstOrDefault(s => s.ProductId == productId);

        return stock is not null
            ? new OkObjectResult(stock)
            : new NotFoundObjectResult($"Stock not found for product {productId}");
    }
}