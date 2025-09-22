using MetaExchange.Core;
using MetaExchange.Core.Enums;
using MetaExchange.Core.Services;
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
if (!Enum.TryParse<Side>(args[1], true, out var side))
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

// Load exchanges
var exchanges = ExchangesLoader.LoadExchanges(folder);

// Calculate execution plan
var planner = new ExecutionPlanner();
var executionPlan = planner.Execute(exchanges, side, amount);

// Serialize to JSON for output
var json = JsonSerializer.Serialize(executionPlan, options: new() { WriteIndented = true });
Console.WriteLine(json);

if (!string.IsNullOrWhiteSpace(output))
{
    File.WriteAllText(output, json);
    Console.WriteLine($"Saved to: {output}");
}