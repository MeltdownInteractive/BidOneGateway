using BidOneGateway.Domain.Models.Erp;

namespace BidOneGateway.Application.Interfaces;

public interface IErpClient
{
    Task<List<ErpProduct>> GetProductsAsync();
}