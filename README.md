# BidOneGateway

This is an Azure Functions (isolated worker model) project written in ASP.NET CORE running on .NET 8.
This project was built in JetBrains Rider 2025.1.5

There is a Postman collection in the folder /Postman Collection

CreateOrUpdateProductV1: [POST] http://localhost:7026/api/v1/products

GetProductByIdV1: [GET] http://localhost:7026/api/v1/products/{id:int}

GetProductsV1: [GET] http://localhost:7026/api/v1/products

StubErpGetProductById: [GET] http://localhost:7026/api/erp/products/{id:int}

StubErpGetProducts: [GET] http://localhost:7026/api/erp/products

StubWarehouseGetStock: [GET] http://localhost:7026/api/warehouse/stock/{productId:int}

I was unable to implement the full solution, as I ran out of time after spending nearly two full days on this.
Given the scope, I focused my time on what I believed were the most critical, high-risk areas: establishing a solid, testable architecture and implementing the complex orchestration logic with robust Polly policies. 
I left the simpler PUT and DELETE endpoints as stubs to respect the timebox, but the pattern for implementing them is clearly established.

Further improvements I'd make..

1.) Using ASP.NET Core API Versioning to decorate the endpoints with the relevant version tags instead of the hard code v1/x endpoint route.

2.) Move the resilience handling configuration into a PollyPolicySettings configuration file and pull the configuration values from there.
