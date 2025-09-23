using MetaExchange.Core;
using MetaExchange.Core.Enums;
using MetaExchange.Core.Models;

namespace MetaExchange.Tests;
public class ExecutionPlannerPrecisionTests
{
    private static Exchange GenerateExchange(
        string id, decimal eur, decimal btc,
        (decimal amt, decimal px)[] asks)
    {
        var askW = asks.Select((t, i) =>
            new WrappedOrder(new Order($"ask-{id}-{i}", DateTime.UtcNow, "Sell", "Limit", t.amt, t.px))).ToList();
        return new Exchange(id, new AvailableFunds(btc, eur), new OrderBook(new(), askW));
    }

    [Fact]
    public void Quantities_AreFloored_To8dp_WhenRequestHas9dp()
    {
        // Abundant EUR & size so only precision caps the quantity.
        var exchanges = new[]
        {
            GenerateExchange("Exchange", eur: 10000000m, btc: 0m, asks: [(10m, 10000m)])
        };
        var requested = 0.123456789m;

        var plan = new ExecutionPlanner().Execute(exchanges, TradeType.BUY, requested);

        Assert.Equal(0.12345678m, plan.FilledAmountBtc);
        Assert.Equal(requested - 0.12345678m, plan.ShortfallBtc);
    }

    [Fact]
    public void Notional_IsRoundedDown_Internally()
    {
        // 1 satoshi at a fractional cent price
        var exchanges = new[]
        {
            GenerateExchange("Exchange", eur: 1000m, btc: 0m, asks: [(0.00000001m, 25000.12m)])
        };

        var plan = new ExecutionPlanner().Execute(exchanges, TradeType.BUY, 0.00000001m);

        var line = plan.Orders.Single();
        // raw: 25_000.12 * 0.00000001 = 0.0002500012
        // internal rounding down to EurInternalDp (8) should not overstate
        Assert.True(line.NotionalEur <= 0.00025001m);
    }

    [Fact]
    public void DoesNot_Overfill_BeyondLevelSize_AfterFlooring()
    {
        // Level size just above the boundary; flooring must not exceed level size.
        var levelSize = 0.100000009m; // will floor to 0.10000000
        var exs = new[]
        {
            GenerateExchange("Ex", eur: 10000000m, btc: 0m, asks: [(levelSize, 10000m)])
        };

        var plan = new ExecutionPlanner().Execute(exs, TradeType.BUY, 0.2m);

        Assert.True(plan.FilledAmountBtc <= 0.10000000m + 0.00000001m);
    }
}
