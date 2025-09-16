using BidOneGateway.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BidOneGateway.Api.Functions;

public class ProductApi(IProductOrchestrationService orchestrationService, ILogger<ProductApi> logger)
{
    [Function("GetProductsV1")]
    public async Task<IActionResult> GetProducts(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/products")] HttpRequest req)
    {
        logger.LogInformation("V1/products GET request received.");
        try
        {
            var products = await orchestrationService.GetMergedProductsAsync();
            return new OkObjectResult(products);
        }
        catch (Exception ex)
        {
            // This catches the re-thrown exception if the critical ERP call fails
            logger.LogError(ex, "A critical error occurred while processing the request for all products.");
            
            // Return a 503 Service Unavailable to indicate a downstream dependency is failing
            return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
        }
    }
    
    [Function("CreateOrUpdateProductV1")]
    public IActionResult CreateOrUpdateProduct(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/products")] HttpRequest req)
    {
        // TODO: Implement idempotency logic using Idempotency-Key header.
        // TODO: Call an orchestration service method to create/update the product in the ERP.
        logger.LogInformation("V1/products POST request received.");
        return new CreatedResult("/v1/products/123", new { message = "Product created/updated successfully." });
    }

    [Function("GetProductByIdV1")]
    public async Task<IActionResult> GetProductById(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/products/{id:int}")] HttpRequest req, int id)
    {
        logger.LogInformation($"V1/products/{id} GET request received for ID {id}.");

        try
        {
            var product = await orchestrationService.GetMergedProductByIdAsync(id);

            return product is not null
                ? new OkObjectResult(product)
                : new NotFoundResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "A critical error occurred while processing the request for product ID {ProductId}.", id);
            return new StatusCodeResult(StatusCodes.Status503ServiceUnavailable);
        }
    }
}