using MetaExchange.Core.Models;
using System.Text.Json;

namespace MetaExchange.Core.Services;
public static class ExchangesLoader
{
    public static IReadOnlyList<Exchange> LoadExchanges(string folder)
    {
        JsonSerializerOptions options = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        return [.. Directory.GetFiles(folder, "*.json")
            .Select(f => JsonSerializer.Deserialize<Exchange>(File.ReadAllText(f), options)
                ?? throw new InvalidOperationException($"Invalid JSON: {f}"))];
    }
}
