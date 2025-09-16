using System.Net.Http.Json;
using BidOneGateway.Application.Interfaces;
using BidOneGateway.Domain.Models.Warehouse;
using Microsoft.Extensions.Logging;

namespace BidOneGateway.Infrastructure.Warehouse.Api.Clients;

public class WarehouseClient(HttpClient httpClient, ILogger<WarehouseClient> logger) : IWarehouseClient
{
    public async Task<WarehouseStock?> GetStockForProductAsync(int productId)
    {
        try
        {
            logger.LogDebug("Calling Warehouse upstream for stock of product ID {ProductId}.", productId);
            var response = await httpClient.GetAsync($"warehouse/stock/{productId}");
            
            if (response.IsSuccessStatusCode)
            {
                var stock = await response.Content.ReadFromJsonAsync<WarehouseStock>();
                logger.LogDebug("Successfully fetched stock for product ID {ProductId}.", productId);
                return stock;
            }

            // A 404 Not Found is a valid business case, not a system failure.
            logger.LogWarning("Failed to get stock for ProductId {ProductId}. Status: {StatusCode}",
                productId, response.StatusCode);
            return null;
        }
        catch (HttpRequestException ex)
        {
            // Catch transient errors if Polly ultimately fails. Treat as a partial failure since we will return 0 for failed stock lookups
            logger.LogError(ex, "Stock lookup request failed for ProductId {ProductId} after all retries.", productId);
            return null;
        }
    }
}