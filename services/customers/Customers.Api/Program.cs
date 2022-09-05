using System.Globalization;
using Customers.Api.Domain;
using Dapr;
using Dapr.Client;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCustomAppInsights();

builder.Services.AddDbContext<CustomersDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SQL") ?? throw new ArgumentException("SQL Connection String missing"));
});

builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = true;
});

builder.Services.AddGrpcReflection();
builder.Services.AddGrpcHttpApi();
builder.Services.AddGrpcSwagger();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<CustomersDbContext>();

builder.Services.AddDaprClient();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();

//app.UseAuthentication();
//app.UseAuthorization();

app.MapCustomHealthCheckEndpoints();

app.MapGrpcService<CustomersService>();
app.MapGrpcReflectionService();

app.MapPost("/publish-event1", async (DaprClient dapr) =>
{
    var evt = new MyEvent1
    {
        Text1 = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
    };
    // We must manually construct the cloud event because the .NET SDK doesn't change the default "type" (com.dapr.event.sent)
    var cloudEvt = new CloudEvent<MyEvent1>(evt)
    {
        Type = nameof(MyEvent1)
    };
    await dapr.PublishEventAsync("pubsub", "test-topic", cloudEvt);
    return Results.Ok();
});

app.MapPost("/publish-event2", async (DaprClient dapr) =>
{
    var evt = new MyEvent2
    {
        Text2 = DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)
    };
    // We must manually construct the cloud event because the .NET SDK doesn't change the default "type" (com.dapr.event.sent)
    var cloudEvt = new CloudEvent<MyEvent2>(evt)
    {
        Type = nameof(MyEvent2)
    };
    await dapr.PublishEventAsync("pubsub", "test-topic", cloudEvt);
    return Results.Ok();
});

app.MapGet("/", () => "Hello World");

app.Run();


record MyEvent1
{
    public string Text1 { get; set; } = string.Empty;
}

record MyEvent2
{
    public string Text2 { get; set; } = string.Empty;
}
