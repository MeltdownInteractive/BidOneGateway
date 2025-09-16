using BidOneGateway.Application.Interfaces;
using BidOneGateway.Application.Services;
using BidOneGateway.Domain.Models.Settings;
using BidOneGateway.Infrastructure.Erp.Api.Clients;
using BidOneGateway.Infrastructure.Warehouse.Api.Clients;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

var host = new HostBuilder()
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.AddMemoryCache();
        
        services.Configure<UpstreamServicesSettings>(
            context.Configuration.GetSection(UpstreamServicesSettings.SectionName)
        );
        
        var resiliencePolicy = HttpPolicyExtensions
            .HandleTransientHttpError() // Handles HttpRequestException, 5xx, 408
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) +
                TimeSpan.FromMilliseconds(new Random().Next(0, 100))
            );
        
        services.AddHttpClient<IErpClient, ErpClient>((serviceProvider, client) =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<UpstreamServicesSettings>>().Value;
                client.BaseAddress = new Uri(settings.ErpUrl);
            })
            .AddPolicyHandler(resiliencePolicy);

        services.AddHttpClient<IWarehouseClient, WarehouseClient>((serviceProvider, client) =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<UpstreamServicesSettings>>().Value;
                client.BaseAddress = new Uri(settings.WarehouseUrl);
            })
            .AddPolicyHandler(resiliencePolicy);
        
        services.AddScoped<IProductOrchestrationService, ProductOrchestrationService>();
    })
    .Build();

host.Run();