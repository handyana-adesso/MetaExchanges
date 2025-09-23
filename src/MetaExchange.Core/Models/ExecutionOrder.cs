using MetaExchange.Core.Enums;

namespace MetaExchange.Core.Models;

public record ExecutionOrder(
    string ExchangeId,
    TradeType Side,
    decimal Price,
    decimal QuantityBtc,
    decimal NotionalEur
);
