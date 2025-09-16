using System.Net.Http.Json;
using BidOneGateway.Application.Interfaces;
using BidOneGateway.Domain.Models.Erp;
using Microsoft.Extensions.Logging;

namespace BidOneGateway.Infrastructure.Erp.Api.Clients;

public class ErpClient(HttpClient httpClient, ILogger<ErpClient> logger) : IErpClient
{
    public async Task<List<ErpProduct>> GetProductsAsync()
    {
        try
        {
            logger.LogInformation("Calling ERP upstream to get all products.");
            var response = await httpClient.GetAsync("erp/products");
            response.EnsureSuccessStatusCode(); // Throws on non-2xx after Polly retries
            
            var products = await response.Content.ReadFromJsonAsync<List<ErpProduct>>();
            logger.LogInformation("Successfully deserialized {ProductCount} products from ERP.", products?.Count ?? 0);
            
            return products ?? new List<ErpProduct>();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "The critical ERP service call failed after all retries.");
            
            // Re-throw to allow the orchestration service to handle the critical failure
            throw; 
        }
    }
}