using BidOneGateway.Domain.Models.Erp;

namespace BidOneGateway.Infrastructure.Erp.Api.Functions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class ErpFunctions(ILogger<ErpFunctions> logger)
{
    private static readonly List<ErpProduct> Products =
    [
        new ErpProduct
        {
            Id = 1, 
            Sku = "WIDGET-001",
            Name = "Super Widget",
            Description = "A high-quality widget for all your widgeting needs."
        },

        new ErpProduct
        {
            Id = 2,
            Sku = "GADGET-007",
            Name = "Power Gadget Pro",
            Description = "The most powerful gadget on the market."
        }
    ];

    [Function("ErpGetProducts")]
    public IActionResult GetProducts(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "erp/products")]
        HttpRequest req)
    {
        logger.LogInformation("STUB ERP: Received request for all products.");
        return new OkObjectResult(Products);
    }

    [Function("ErpGetProductById")]
    public IActionResult GetProductById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "erp/products/{id:int}")]
        HttpRequest req, int id)
    {
        logger.LogInformation("STUB ERP: Received request for product with ID {Id}", id);

        var product = Products.FirstOrDefault(p => p.Id == id);

        return product is not null
            ? new OkObjectResult(product)
            : new NotFoundResult();
    }

    [Function("ErpCreateOrUpdateProduct")]
    public IActionResult CreateOrUpdateProduct(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", "put", Route = "erp/products")]
        HttpRequest req)
    {
        logger.LogInformation("STUB ERP: Received POST/PUT to create/update a product. Assuming success.");

        // Simulate creating a new product with a new ID
        var newId = Products.Max(p => p.Id) + 1;
        var createdProduct = new ErpProduct
        {
            Id = newId,
            Sku = $"NEW-SKU-{newId}",
            Name = "Newly Created Product",
            Description = "Created via the stub."
        };
        
        return new CreatedResult($"/erp/products/{createdProduct.Id}", createdProduct);
    }
}