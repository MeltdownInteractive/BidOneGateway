using BidOneGateway.Application.Interfaces;
using BidOneGateway.Domain.Models.Dto;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BidOneGateway.Application.Services;

public class ProductOrchestrationService(
    IErpClient erpClient,
    IWarehouseClient warehouseClient,
    ILogger<ProductOrchestrationService> logger,
    IMemoryCache memoryCache)
    : IProductOrchestrationService
{
    public async Task<IEnumerable<ProductDto>?> GetMergedProductsAsync()
    {
        const string cacheKey = "AllProducts";
        logger.LogInformation("Checking cache for all products.");

        // Try to get from cache first
        if (memoryCache.TryGetValue(cacheKey, out IEnumerable<ProductDto>? cachedProducts))
        {
            logger.LogInformation("Cache hit for all products. Returning cached data.");
            return cachedProducts;
        }
        
        logger.LogInformation("Cache miss. Starting product merge orchestration.");
        logger.LogInformation("Starting product merge orchestration.");
        
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

        // Merge results, return 0 stock if stock lookup fails
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
        logger.LogInformation("Storing merged products in cache.");
        
        var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
        var mergedProductsAsync = mergedProducts as ProductDto[] ?? mergedProducts.ToArray();
        memoryCache.Set(cacheKey, mergedProductsAsync, cacheEntryOptions);
        
        logger.LogInformation("Successfully stored merged products in cache.");
        
        return mergedProductsAsync;
    }
    
    public async Task<ProductDto?> GetMergedProductByIdAsync(int id)
    {
        logger.LogInformation("Starting orchestration to get merged product for ID {ProductId}.", id);
        
        var erpProduct = await erpClient.GetProductByIdAsync(id);
        
        if (erpProduct is null)
        {
            logger.LogWarning("Orchestration ended: Product with ID {ProductId} not found in ERP.", id);
            return null;
        }
        
        var warehouseStock = await warehouseClient.GetStockForProductAsync(id);
        
        var mergedProduct = new ProductDto
        {
            Id = erpProduct.Id,
            Sku = erpProduct.Sku,
            Name = erpProduct.Name,
            StockLevel = warehouseStock?.StockLevel ?? 0
        };
        
        logger.LogInformation("Successfully merged data for product ID {ProductId}.", id);
        return mergedProduct;
    }
}