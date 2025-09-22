using MetaExchange.Core.Enums;

namespace MetaExchange.Core.Models;

public record ExecutionOrder(
    string ExchangeId,
    Side Side,
    decimal Price,
    decimal QuantityBtc,
    decimal NotionalEur
);
