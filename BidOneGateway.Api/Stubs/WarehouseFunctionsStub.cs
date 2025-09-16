using BidOneGateway.Domain.Models.Warehouse;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BidOneGateway.Api.Stubs;

public class WarehouseFunctionsStub(ILogger<WarehouseFunctionsStub> logger)
{
    private static readonly List<WarehouseStock> StockLevels =
    [
        new WarehouseStock { ProductId = 1, StockLevel = 150, WarehouseLocation = "Aisle 12" },
        new WarehouseStock { ProductId = 2, StockLevel = 0, WarehouseLocation = "Aisle 3" }
    ];

    [Function("StubWarehouseGetStock")]
    public IActionResult GetStockByProductId(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "warehouse/stock/{productId:int}")] HttpRequest req, int productId)
    {
        logger.LogWarning("--- STUB WAREHOUSE SERVICE CALLED: GET warehouse/stock/{id} ---", productId);
        var stock = StockLevels.FirstOrDefault(s => s.ProductId == productId);
        return stock is not null ? new OkObjectResult(stock) : new NotFoundObjectResult($"Stock not found for product {productId}");
    }
}