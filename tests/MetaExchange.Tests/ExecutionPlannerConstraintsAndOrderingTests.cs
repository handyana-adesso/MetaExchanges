using MetaExchange.Core;
using MetaExchange.Core.Enums;
using MetaExchange.Core.Models;

namespace MetaExchange.Tests;
public class ExecutionPlannerConstraintsAndOrderingTests
{
    private static Exchange GenerateExchange(
        string id, decimal eur, decimal btc,
        (decimal amt, decimal px)[] asks,
        (decimal amt, decimal px)[] bids)
    {
        var askW = asks.Select((t, i) =>
            new WrappedOrder(new Order($"ask-{id}-{i}", DateTime.UtcNow, "Sell", "Limit", t.amt, t.px))).ToList();
        var bidW = bids.Select((t, i) =>
            new WrappedOrder(new Order($"bid-{id}-{i}", DateTime.UtcNow, "Buy", "Limit", t.amt, t.px))).ToList();

        return new Exchange(id, new AvailableFunds(btc, eur), new OrderBook(bidW, askW));
    }

    [Fact]
    public void Buy_RespectsPerExchangeEUR_NoCrossSubsidy()
    {
        // ExchangeA cheap but only 500 EUR → max 0.05 BTC; ExB pricier but enough EUR.
        var exchanges = new[]
        {
            GenerateExchange("ExchangeA", eur: 500m, btc: 0m, asks: [(1.0m, 10000m)], bids: []),
            GenerateExchange("ExchangeB", eur: 10000m, btc: 0m, asks: [(1.0m, 10050m)], bids: []),
        };

        var plan = new ExecutionPlanner().Execute(exchanges, Side.BUY, 0.2m);
        // Ensure we didn't buy more than 0.05 on ExchangeA
        var exAOrder = plan.Orders.FirstOrDefault(o => o.ExchangeId == "ExchangeA");
        Assert.NotNull(exAOrder);
        Assert.True(exAOrder!.QuantityBtc <= 0.05m + 0.00000001m);
    }

    [Fact]
    public void Sell_RespectsPerExchangeBTC()
    {
        // ExchangeA has only 0.05 BTC, though price is best. Need 0.2 total.
        var exchanges = new[]
        {
            GenerateExchange("ExchangeA", eur: 0m, btc: 0.05m, asks: [], bids: [(1.0m, 20200m)]),
            GenerateExchange("ExchangeB", eur: 0m, btc: 0.2m,  asks: [], bids: [(1.0m, 20000m)]),
        };

        var plan = new ExecutionPlanner().Execute(exchanges, Side.SELL, 0.2m);
        var exA = plan.Orders.FirstOrDefault(o => o.ExchangeId == "ExchangeA");
        var exB = plan.Orders.FirstOrDefault(o => o.ExchangeId == "ExchangeB");

        Assert.True(exA!.QuantityBtc <= 0.05m + 0.00000001m);  // capped by BTC balance
        Assert.True(exB!.QuantityBtc >= 0.14999999m);    // remainder sold at ExchangeB
    }

    [Fact]
    public void Ignores_NonPositive_Prices_And_Sizes()
    {
        var exchanges = new[]
        {
            GenerateExchange("BadPrice", eur: 1000m, btc: 0m, asks: [(0.1m, 0m)], bids: []),
            GenerateExchange("BadSize", eur: 1000m, btc: 0m, asks: [(0m, 10000m)], bids: []),
            GenerateExchange("Good",  eur: 1000m, btc: 0m, asks: [(0.1m, 10000m)], bids: []),
        };

        var plan = new ExecutionPlanner().Execute(exchanges, Side.BUY, 0.05m);

        Assert.Single(plan.Orders);
        Assert.Equal("Good", plan.Orders[0].ExchangeId);
    }

    [Fact]
    public void Shortfall_WhenNoLiquidity()
    {
        var exchanges = new[]
        {
            GenerateExchange("Exchange1", eur: 1000000m, btc: 0m, asks: [], bids: []),
        };

        var plan = new ExecutionPlanner().Execute(exchanges, Side.BUY, 1.0m);
        Assert.Equal(0m, plan.FilledAmountBtc);
        Assert.Equal(1.0m, plan.ShortfallBtc);
        Assert.Empty(plan.Orders);
    }

    [Fact]
    public void Chooses_HighestBids_ForSell_And_LowestAsks_ForBuy()
    {
        var exchanges = new[]
        {
            GenerateExchange("ExchangeA", eur: 10_000m, btc: 0.5m,
               asks: [(1.0m, 10200m), (1.0m, 10100m)],  // unsorted on purpose
               bids: [(1.0m, 9800m), (1.0m, 9900m)]),
        };

        var buy = new ExecutionPlanner().Execute(exchanges, Side.BUY, 0.1m);
        Assert.Equal(10100m, buy.Orders[0].Price); // lowest ask first

        var sell = new ExecutionPlanner().Execute(exchanges, Side.SELL, 0.1m);
        Assert.Equal(9900m, sell.Orders[0].Price); // highest bid first
    }
}
