using MetaExchange.Core;
using MetaExchange.Core.Abstractions;
using MetaExchange.Core.Enums;
using MetaExchange.Core.Services;

using Microsoft.Extensions.DependencyInjection;

using System.Globalization;
using System.Text.Json;

if (args.Length < 3)
{
    Console.WriteLine("Usage: MetaExchange.CLI <orderbooksFolder> <BUY|SELL> <amountBtc> [outputJsonPath]");
    Console.WriteLine("Example: dotnet run ../../orderbooks BUY 1.0 plan.json");
    return;
}

// Parse CLI arguments
var folder = args[0];
if (!Enum.TryParse<TradeType>(args[1], true, out var side))
{
    Console.WriteLine("Invalid side: use BUY or SELL.");
    return;
}

if (!decimal.TryParse(args[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var amount) 
    || amount <= 0)
{
    Console.WriteLine("Invalid amountBtc.");
    return;
}

var output = args.Length > 3 ? args[3] : null;

// DI Container
var services = new ServiceCollection()
    .AddSingleton<IExchangesLoader, ExchangesLoader>()
    .AddSingleton<IExecutionPlanner, ExecutionPlanner>()
    .BuildServiceProvider();

var loader = services.GetRequiredService<IExchangesLoader>();
var planner = services.GetRequiredService<IExecutionPlanner>();

// Load exchanges
var exchanges = await loader.LoadExchangesAsync(folder);

// Calculate execution plan
var executionPlan = planner.Execute(exchanges, side, amount);

// Serialize to JSON for output
var json = JsonSerializer.Serialize(executionPlan, options: new() { WriteIndented = true });
Console.WriteLine(json);

if (!string.IsNullOrWhiteSpace(output))
{
    File.WriteAllText(output, json);
    Console.WriteLine($"Saved to: {output}");
}