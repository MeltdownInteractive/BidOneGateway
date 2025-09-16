using BidOneGateway.Domain.Models;
using BidOneGateway.Domain.Models.Erp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace BidOneGateway.Api.Stubs;

public class ErpFunctionsStub(ILogger<ErpFunctionsStub> logger)
{
    private static readonly List<ErpProduct> Products =
    [
        new ErpProduct { Id = 1, Sku = "WIDGET-001", Name = "Super Widget", Description = "A high-quality widget." },
        new ErpProduct { Id = 2, Sku = "GADGET-007", Name = "Power Gadget Pro", Description = "The most powerful gadget." }
    ];

    [Function("StubErpGetProducts")]
    public IActionResult GetProducts(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "erp/products")] HttpRequest req)
    {
        logger.LogInformation("--- STUB ERP SERVICE CALLED: GET erp/products ---");
        return new OkObjectResult(Products);
    }

    [Function("StubErpGetProductById")]
    public IActionResult GetProductById(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "erp/products/{id:int}")] HttpRequest req, int id)
    {
        logger.LogInformation("--- STUB ERP SERVICE CALLED: GET erp/products/{id} ---", id);
        var product = Products.FirstOrDefault(p => p.Id == id);
        return product is not null ? new OkObjectResult(product) : new NotFoundResult();
    }
}