using MetaExchange.Core.Enums;
using MetaExchange.Core.Helpers;
using MetaExchange.Core.Models;

namespace MetaExchange.Core;

public sealed class ExecutionPlanner
{
    public ExecutionPlan Execute(
        IReadOnlyList<Exchange> exchanges,
        Side side,
        decimal amountBtc)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amountBtc);

        var eurDict = exchanges.ToDictionary(e => e.Id, e => e.AvailableFunds.Euro);
        var btcDict = exchanges.ToDictionary(e => e.Id, e => e.AvailableFunds.Crypto);

        var priceLevels = BuildPriceLevels(exchanges, side)
            .Where(p => p.Price > 0 && p.Size > 0);

        priceLevels = SortPriceLevels(priceLevels, side);

        var orders = new List<ExecutionOrder>();
        decimal remaining = amountBtc, filled = 0, notional = 0;

        foreach (var priceLevel in priceLevels)
        {
            if (remaining <= 0)
            {
                break;
            }

            decimal quantity = 0m;
            if (side == Side.BUY)
            {
                var maxByMoney = priceLevel.Price > 0 ? eurDict[priceLevel.ExchangeId] / priceLevel.Price : 0;
                quantity = Math.Min(remaining, Math.Min(priceLevel.Size, maxByMoney));
            }
            else
            {
                quantity = Math.Min(remaining, Math.Min(priceLevel.Size, btcDict[priceLevel.ExchangeId]));
            }

            quantity = Precision.FloorToStep(quantity, Precision.BtcStep);
            
            if (quantity <= 0)
            {
                continue;
            }

            var lineNotional = Math.Round(priceLevel.Price * quantity, Precision.EurInternalDp, MidpointRounding.ToZero);

            orders.Add(new(priceLevel.ExchangeId, side, priceLevel.Price, quantity, lineNotional));

            if (side == Side.BUY)
            {
                eurDict[priceLevel.ExchangeId] -= lineNotional;
                btcDict[priceLevel.ExchangeId] += quantity;
            }
            else
            {
                eurDict[priceLevel.ExchangeId] += lineNotional;
                btcDict[priceLevel.ExchangeId] -= quantity;
            }

            remaining -= quantity;
            filled += quantity;
            notional += lineNotional;
        }

        var weighedAvgPrice = filled > 0 ? notional / filled : 0m;
        var postTradeBalances = eurDict.Keys
            .Select(k => new PostTradeBalance(k, eurDict[k], btcDict[k]))
            .ToList();

        return new(
            side,
            amountBtc,
            filled,
            amountBtc - filled,
            weighedAvgPrice,
            notional,
            orders,
            postTradeBalances);
    }

    private static IEnumerable<PriceLevel> BuildPriceLevels(
        IReadOnlyList<Exchange> exchanges,
        Side side)
    {
        return side == Side.BUY
            ? exchanges.SelectMany(e => e.OrderBook.Asks
                .Select(a => new PriceLevel(e.Id, a.Order.Price, a.Order.Amount)))
            : exchanges.SelectMany(e => e.OrderBook.Bids
                .Select(b => new PriceLevel(e.Id, b.Order.Price, b.Order.Amount)));
    }

    private static IEnumerable<PriceLevel> SortPriceLevels(
        IEnumerable<PriceLevel> priceLevels,
        Side side)
    {
        return side == Side.BUY
            ? priceLevels.OrderBy(p => p.Price).ThenByDescending(p => p.Size)
            : priceLevels.OrderByDescending(p => p.Price).ThenByDescending(p => p.Size);
    }
}
