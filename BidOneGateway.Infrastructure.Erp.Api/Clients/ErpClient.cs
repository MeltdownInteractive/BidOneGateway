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
    
    public async Task<ErpProduct?> GetProductByIdAsync(int id)
    {
        try
        {
            logger.LogInformation("Calling ERP upstream to get product by ID {ProductId}.", id);
            
            var response = await httpClient.GetAsync($"erp/products/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    logger.LogWarning("Product with ID {ProductId} not found in ERP.", id);
                    return null;
                }
                
                response.EnsureSuccessStatusCode();
            }
            
            var product = await response.Content.ReadFromJsonAsync<ErpProduct>();
            logger.LogInformation("Successfully deserialized product ID {ProductId} from ERP.", id);
            
            return product;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "The ERP service call for product ID {ProductId} failed after all retries.", id);
            throw;
        }
    }
}