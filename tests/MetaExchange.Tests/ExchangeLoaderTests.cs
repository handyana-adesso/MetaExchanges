using MetaExchange.Core.Abstractions;
using MetaExchange.Core.Models;
using MetaExchange.Core.Services;
using System.Text.Json;

namespace MetaExchange.Tests;
public class ExchangeLoaderTests
{
    public async Task LoadFromFolder_Reads_AllJsonFiles()
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

            await File.WriteAllTextAsync(Path.Combine(tmp, "exchangeA.json"), json);
            await File.WriteAllTextAsync(Path.Combine(tmp, "exchangeB.json"), json);

            IExchangesLoader loader = new ExchangesLoader();
            var list = await loader.LoadExchangesAsync(tmp);

            Assert.Equal(2, list.Count);
            Assert.All(list, e => Assert.Equal("ExchangeA", e.Id));
        }
        finally
        {
            Directory.Delete(tmp, recursive: true);
        }
    }

    [Fact]
    public async Task LoadFromFolder_Throws_OnInvalidJson()
    {
        var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tmp);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(tmp, "bad.json"), "{not-json");
            IExchangesLoader loader = new ExchangesLoader();
            await Assert.ThrowsAsync<JsonException>(() => loader.LoadExchangesAsync(tmp));
        }
        finally
        {
            // Retry deletion in case of lingering file locks
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    Directory.Delete(tmp, true);
                    break;
                }
                catch (IOException)
                {
                    // Wait briefly and try again
                    await Task.Delay(50);
                }
            }
        }
    }
}
