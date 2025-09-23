using MetaExchange.Core.Enums;

namespace MetaExchange.Core.Models;

public record ExecutionOrder(
    string ExchangeId,
    TradeType TradeType,
    decimal Price,
    decimal QuantityBtc,
    decimal NotionalEur
);
