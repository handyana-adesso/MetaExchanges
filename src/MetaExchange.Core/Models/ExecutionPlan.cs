using MetaExchange.Core.Enums;

namespace MetaExchange.Core.Models;
public record ExecutionPlan(
    TradeType TradeType,
    decimal RequestedAmountBtc,
    decimal FilledAmountBtc,
    decimal ShortfallBtc,
    decimal WeightedAveragePrice,
    decimal TotalNotionalEuro,
    List<ExecutionOrder> Orders,
    List<PostTradeBalance> PostTradeBalances
);
