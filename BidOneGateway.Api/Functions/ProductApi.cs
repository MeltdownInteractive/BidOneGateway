using System.Collections.Concurrent;
using BidOneGateway.Api.Helpers;
using BidOneGateway.Application.Interfaces;
using BidOneGateway.Domain.Models.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BidOneGateway.Api.Functions;

public class ProductApi(IProductOrchestrationService orchestrationService, ILogger<ProductApi> logger)
{
    private static readonly ConcurrentDictionary<string, IActionResult> IdempotencyCache = new();
    
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
    public async Task<IActionResult> CreateOrUpdateProduct(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/products")] HttpRequest req)
    {
        logger.LogInformation("V1/products POST request received.");
        
        // Ensure the stream is seekable for generating the idempotency signature 
        req.EnableBuffering();
        
        var idempotencySignature = await IdempotencyHelper.GenerateIdempotencySignature(req, "POST", "/v1/products");
        if (idempotencySignature is null)
        {
            return new BadRequestObjectResult("Idempotency-Key header is required.");
        }
        
        if (IdempotencyCache.TryGetValue(idempotencySignature, out var cachedResult))
        {
            logger.LogWarning("Idempotency signature {Signature} found in cache. Returning original response.", idempotencySignature);
            return cachedResult;
        }
        
        logger.LogInformation("Processing new request for idempotency signature {Signature}.", idempotencySignature);
        
        var result = new CreatedResult("/v1/products/456", new { message = "Product created successfully." });
        
        IdempotencyCache.TryAdd(idempotencySignature, result);

        return result;
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