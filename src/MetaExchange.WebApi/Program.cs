using MetaExchange.Core;
using MetaExchange.Core.Abstractions;
using MetaExchange.Core.Services;
using MetaExchange.WebApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IExchangesLoader, ExchangesLoader>();
builder.Services.AddSingleton<IExecutionPlanner, ExecutionPlanner>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { Status = "Ok" }));

// Main trading endpoint
app.MapPost("/execute/{side}/{amountBtc:decimal}", 
    async ([AsParameters]ExecutionPlanRequest request, 
        IExchangesLoader loader,
        IExecutionPlanner planner,
        IConfiguration config,
        CancellationToken cancellationToken
) =>
{
    var folder = config["OrderbooksFolder"] ?? "./orderbooks";
    var exchanges = await loader.LoadExchangesAsync(folder, cancellationToken);
    var executionPlan = planner.Execute(exchanges, request.Side, request.AmountBtc);
    return Results.Ok(executionPlan);
})
.WithName("ExecuteBestPlan")
.WithOpenApi();

app.Run();
