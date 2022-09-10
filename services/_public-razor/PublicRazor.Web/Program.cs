using Dapr.Client;
using InternalGrpc.Api;
using InternalGrpcSqlBus.Api;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// ASP.NET Core
builder.Services.AddRazorPages();

// Application Insights
builder.Services.AddCustomAppInsights();

// gRPC Clients
var internalGrpcInvoker = DaprClient.CreateInvocationInvoker("internal-grpc"); // invoker should be singleton according to docs
var internalGrpcSqlBusInvoker = DaprClient.CreateInvocationInvoker("internal-grpc-sql-bus");
builder.Services.AddTransient(_ => new InternalGrpcEntities.InternalGrpcEntitiesClient(internalGrpcInvoker));
builder.Services.AddTransient(_ => new Customers.CustomersClient(internalGrpcSqlBusInvoker));

// HTTP Clients
var baseUrl = (Environment.GetEnvironmentVariable("BASE_URL") ?? "http://localhost") + ":" + (Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3500");
builder.Services.AddHttpClient("internal-http-bus", (httpClient) =>
{
    httpClient.BaseAddress = new Uri(baseUrl);
    httpClient.DefaultRequestHeaders.Add("dapr-app-id", "internal-http-bus");
});

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();


// Configure the HTTP request pipeline.

app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

// Health checks
app.MapCustomHealthCheckEndpoints();

app.Run();
