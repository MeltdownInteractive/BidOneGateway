using BidOneGateway.Domain.Models.Dto;

namespace BidOneGateway.Application.Interfaces;

public interface IProductOrchestrationService
{
    Task<IEnumerable<ProductDto>> GetMergedProductsAsync();
}