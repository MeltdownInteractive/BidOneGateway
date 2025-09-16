using BidOneGateway.Application.Interfaces;
using BidOneGateway.Domain.Models.Dto;
using Microsoft.Extensions.Logging;

namespace BidOneGateway.Application.Services;

public class ProductOrchestrationService(
    IErpClient erpClient,
    IWarehouseClient warehouseClient,
    ILogger<ProductOrchestrationService> logger)
    : IProductOrchestrationService
{
    public async Task<IEnumerable<ProductDto>> GetMergedProductsAsync()
    {
        logger.LogInformation("Starting product merge orchestration.");

        // Get critical product data. This will throw if it fails after all retries.
        var erpProducts = await erpClient.GetProductsAsync();
        if (erpProducts.Count == 0)
        {
            logger.LogWarning("ERP returned no products. Returning empty list.");
            return Enumerable.Empty<ProductDto>();
        }
        
        logger.LogInformation("Successfully fetched {ProductCount} products from ERP. Fetching stock levels.", erpProducts.Count);

        // Get stock for all products concurrently
        var stockLookupTasks = erpProducts
            .Select(p => warehouseClient.GetStockForProductAsync(p.Id))
            .ToList();

        var stockResults = await Task.WhenAll(stockLookupTasks);
        logger.LogInformation("Finished fetching all stock levels.");

        // Merge results, returning 0 stock if stock lookup fails
        var mergedProducts = erpProducts.Select((product, index) =>
        {
            var stockInfo = stockResults[index];
            return new ProductDto
            {
                Id = product.Id,
                Sku = product.Sku,
                Name = product.Name,
                StockLevel = stockInfo?.StockLevel ?? 0
            };
        });
        
        logger.LogInformation("Successfully merged all product and stock data.");
        return mergedProducts;
    }
}