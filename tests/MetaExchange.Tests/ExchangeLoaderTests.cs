using MetaExchange.Core.Models;
using MetaExchange.Core.Services;
using System.Text.Json;

namespace MetaExchange.Tests;
public class ExchangeLoaderTests
{
    [Fact]
    public void LoadFromFolder_Reads_AllJsonFiles()
    {
        var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);

        try
        {
            var json = JsonSerializer.Serialize(new Exchange(
                "ExchangeA",
                new AvailableFunds(0.1m, 1000m),
                new OrderBook(new(), new()
                {
                    new(new Order("ask", DateTime.UtcNow, "Sell", "Limit", 0.05m, 10_000m))
                })
            ));

            File.WriteAllText(Path.Combine(tmp, "exchangeA.json"), json);
            File.WriteAllText(Path.Combine(tmp, "exchangeB.json"), json);

            var list = ExchangesLoader.LoadExchanges(tmp);

            Assert.Equal(2, list.Count);
            Assert.All(list, e => Assert.Equal("ExchangeA", e.Id));
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }

    [Fact]
    public void LoadFromFolder_Throws_OnInvalidJson()
    {
        var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);

        try
        {
            File.WriteAllText(Path.Combine(tmp, "bad.json"), "{not-json");
            Assert.Throws<JsonException>(() => ExchangesLoader.LoadExchanges(tmp));
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }
}
