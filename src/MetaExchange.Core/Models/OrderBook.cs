namespace MetaExchange.Core.Models;
public record OrderBook(
    List<WrappedOrder> Bids,
    List<WrappedOrder> Asks
);
