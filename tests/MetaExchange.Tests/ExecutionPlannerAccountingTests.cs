using MetaExchange.Core;
using MetaExchange.Core.Enums;
using MetaExchange.Core.Models;

namespace MetaExchange.Tests;

public class ExecutionPlannerAccountingTests
{
    private static Exchange GenerateExchange(
        string id, decimal eur, decimal btc,
        (decimal amt, decimal px)[] asks,
        (decimal amt, decimal px)[] bids)
    {
        var askWrappedOrder = asks.Select((t, i) =>
            new WrappedOrder(new Order($"ask-{id}-{i}", DateTime.UtcNow, "Sell", "Limit", t.amt, t.px))).ToList();
        var bidWrappedOrder = bids.Select((t, i) =>
            new WrappedOrder(new Order($"bid-{id}-{i}", DateTime.UtcNow, "Buy", "Limit", t.amt, t.px))).ToList();

        return new Exchange(id, new AvailableFunds(btc, eur), new OrderBook(bidWrappedOrder, askWrappedOrder));
    }

    [Fact]
    public void Buy_DebitsEUR_AddsBTC_Correctly_PerExchange()
    {
        // ExA is cheaper; both have EUR. Request 0.3 BTC.
        var exchanges = new[]
        {
            GenerateExchange("ExchangeA", eur: 6000m, btc: 0m, asks: new[]{ (0.2m, 10000m) }, bids: Array.Empty<(decimal,decimal)>()),
            GenerateExchange("ExchangeB", eur: 6000m, btc: 0m, asks: new[]{ (1.0m, 10050m) }, bids: Array.Empty<(decimal,decimal)>()),
        };

        var plan = new ExecutionPlanner().Execute(exchanges, TradeType.BUY, 0.3m);

        // Expect 0.2 @ ExA + 0.1 @ ExB
        var exchangeA = plan.PostTradeBalances.Single(b => b.ExchangeId == "ExchangeA");
        var exchangeB = plan.PostTradeBalances.Single(b => b.ExchangeId == "ExchangeB");

        // EUR debited using internal rounding (down). BTC credited with exact qty.
        Assert.True(exchangeA.Euro <= 6000m - (0.2m * 10000m) + 0.00000001m);
        Assert.Equal(0.2m, exchangeA.Crypto);

        Assert.True(exchangeB.Euro <= 6000m - (0.1m * 10050m) + 0.00000001m);
        Assert.Equal(0.1m, exchangeB.Crypto);
    }

    [Fact]
    public void Sell_CreditsEUR_SubtractsBTC_Correctly_PerExchange()
    {
        // Two exchanges with BTC; ExA has better bid.
        var exchanges = new[]
        {
            GenerateExchange("ExchangeA", eur: 0m, btc: 0.25m, asks: [], bids: [(0.2m, 20200m)]),
            GenerateExchange("ExchangeB", eur: 0m, btc: 0.25m, asks: [], bids: [(0.2m, 20150m)]),
        };

        var plan = new ExecutionPlanner().Execute(exchanges, TradeType.SELL, 0.3m);

        // Expect 0.2 sold at ExA, and 0.1 at ExB
        var exchangeA = plan.PostTradeBalances.Single(b => b.ExchangeId == "ExchangeA");
        var exchangeB = plan.PostTradeBalances.Single(b => b.ExchangeId == "ExchangeB");

        Assert.Equal(0.25m - 0.2m, exchangeA.Crypto);
        Assert.True(exchangeA.Euro >= 0.2m * 20200m - 0.00000001m);

        Assert.Equal(0.25m - 0.1m, exchangeB.Crypto);
        Assert.True(exchangeB.Euro >= 0.1m * 20150m - 0.00000001m);
    }

    [Fact]
    public void WeightedAveragePrice_ComputedCorrectly()
    {
        // 0.3 @ 10000 and 0.2 @ 10100
        var exchanges = new[]
        {
            GenerateExchange("ExchangeA", 10000m, 0m, asks: [(0.3m, 10000m)], bids: []),
            GenerateExchange("ExchangeB", 10000m, 0m, asks: [(0.2m, 10100m)], bids: []),
        };

        var plan = new ExecutionPlanner().Execute(exchanges, TradeType.BUY, 0.5m);
        var expectedNotional = 0.3m * 10000m + 0.2m * 10100m;
        var expectedWap = expectedNotional / 0.5m;

        Assert.Equal(0.5m, plan.FilledAmountBtc);
        Assert.InRange((double)Math.Abs(plan.TotalNotionalEuro - expectedNotional), 0, 0.000001);
        Assert.InRange((double)Math.Abs(plan.WeightedAveragePrice - expectedWap), 0, 0.000001);
    }
}