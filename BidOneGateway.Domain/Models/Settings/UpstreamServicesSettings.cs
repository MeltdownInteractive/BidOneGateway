namespace BidOneGateway.Domain.Models.Settings;

public class UpstreamServicesSettings
{
    public const string SectionName = "UpstreamServices";
    public string ErpUrl { get; set; } = string.Empty;
    public string WarehouseUrl { get; set; } = string.Empty;
}