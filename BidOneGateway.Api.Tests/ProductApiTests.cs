using System.Collections.Concurrent;
using System.Reflection;
using BidOneGateway.Api.Functions;
using BidOneGateway.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace BidOneGateway.Api.Tests;

public class ProductApiTests
{
    private readonly Mock<IProductOrchestrationService> _mockOrchestrationService;
    private readonly Mock<ILogger<ProductApi>> _mockLogger;
    private readonly ProductApi _sut;

    public ProductApiTests()
    {
        _mockOrchestrationService = new Mock<IProductOrchestrationService>();
        _mockLogger = new Mock<ILogger<ProductApi>>();
        _sut = new ProductApi(_mockOrchestrationService.Object, _mockLogger.Object);

        // Use Reflection to reset the static idempotency cache for each test
        var idempotencyCacheField = typeof(ProductApi).GetField("_idempotencyCache", BindingFlags.NonPublic | BindingFlags.Static);
        if (idempotencyCacheField != null)
        {
            var cache = idempotencyCacheField.GetValue(null) as ConcurrentDictionary<string, IActionResult>;
            cache?.Clear();
        }
    }
    
    private Mock<HttpRequest> CreateMockRequest(Dictionary<string, StringValues>? headers = null)
    {
        var mockRequest = new Mock<HttpRequest>();
        var mockHttpContext = new Mock<HttpContext>();

        // Set up the HttpContext
        mockRequest.Setup(r => r.HttpContext).Returns(mockHttpContext.Object);
        
        // Set up the Body (required for EnableBuffering)
        mockRequest.Setup(r => r.Body).Returns(new MemoryStream());

        // Set up the headers
        var headerDictionary = headers != null ? new HeaderDictionary(headers) : new HeaderDictionary();
        mockRequest.Setup(r => r.Headers).Returns(headerDictionary);

        return mockRequest;
    }

    [Fact]
    public async Task CreateOrUpdateProduct_WithSameIdempotencyKey_ShouldReturnCachedResultOnSecondCall()
    {
        // Arrange
        var idempotencyKey = Guid.NewGuid().ToString();
        var headers = new Dictionary<string, StringValues> { { "Idempotency-Key", idempotencyKey } };
        
        var mockRequest1 = CreateMockRequest(headers);
        var mockRequest2 = CreateMockRequest(headers);

        // Act
        var firstResult = await _sut.CreateOrUpdateProduct(mockRequest1.Object);
        var secondResult = await _sut.CreateOrUpdateProduct(mockRequest2.Object);

        // Assert
        Assert.IsType<CreatedResult>(firstResult);
        
        // The second result should be the same object from the cache
        Assert.Same(firstResult, secondResult); 
    }

    [Fact]
    public async Task CreateOrUpdateProduct_WithoutIdempotencyKey_ShouldReturnBadRequest()
    {
        // Arrange
        var mockRequest = CreateMockRequest();

        // Act
        var result = await _sut.CreateOrUpdateProduct(mockRequest.Object);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        
        Assert.Equal("Idempotency-Key header is required.", badRequestResult.Value);
    }
}