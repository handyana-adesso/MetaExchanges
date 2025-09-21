namespace MetaExchange.Core.Models;
public record Order(
    string Id,
    DateTime Time,
    string Type,
    string Kind,
    decimal Amount,
    decimal Price
);
