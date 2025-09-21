namespace MetaExchange.Core.Models;
public record PostTradeBalance(
    string ExchangeId,
    decimal Euro,
    decimal Crypto
);
