namespace MetaExchange.Core.Models;
public record PriceLevel(
    string ExchangeId,
    decimal Price,
    decimal Size
);
