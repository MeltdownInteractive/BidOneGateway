using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BidOneGateway.Api.Functions;

public class ProductFunctions(ILogger<ProductFunctions> logger)
{
    #region V1 Endpoints
    [Function("GetProductsV1")]
    public IActionResult GetProductsV1(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/products")] HttpRequest req)
    {
        logger.LogInformation("V1: Getting all products.");
        return new OkObjectResult("This is the V1 list of products.");
    }
    
    [Function("CreateProductV1")]
    public IActionResult CreateProductV1(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1/products")] HttpRequest req)
    {
        logger.LogInformation("V1: Creating a new product.");
        return new CreatedResult("/v1/products/123", "V1 Product created.");
    }
    
    [Function("GetProductByIdV1")]
    public IActionResult GetProductByIdV1(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v1/products/{id}")] HttpRequest req, string id)
    {
        logger.LogInformation("V1: Getting product by id {Id}", id);
        return new OkObjectResult($"This is V1 product with id {id}.");
    }
    
    [Function("UpdateProductByIdV1")]
    public IActionResult UpdateProductByIdV1(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "v1/products/{id}")] HttpRequest req, string id)
    {
        logger.LogInformation("V1: Updating product by id {Id}", id);
        return new OkObjectResult($"Updated V1 product with id {id}.");
    }
    
    [Function("DeleteProductByIdV1")]
    public IActionResult DeleteProductByIdV1(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "v1/products/{id}")] HttpRequest req, string id)
    {
        logger.LogInformation("V1: Deleting product by id {Id}", id);
        return new OkObjectResult($"Deleted V1 product with id {id}.");
    }
    #endregion

    #region V2 Endpoints
    [Function("GetProductsV2")]
    public IActionResult GetProductsV2(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "v2/products")] HttpRequest req)
    {
        logger.LogInformation("V2: Getting all products.");
        return new OkObjectResult("This is the V2 list of products with new fields.");
    }

    [Function("CreateProductV2")]
    public IActionResult CreateProductV2(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "v2/products")] HttpRequest req)
    {
        logger.LogInformation("V2: Creating a new product.");
        return new CreatedResult("/v2/products/123", "V2 Product created.");
    }
    #endregion
}