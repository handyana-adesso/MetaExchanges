namespace MetaExchange.Core.Models;
public record Exchange(
    string Id,
    AvailableFunds AvailableFunds,
    OrderBook OrderBook
);
