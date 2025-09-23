using MetaExchange.Core.Abstractions;
using MetaExchange.Core.Models;
using System.Text.Json;

namespace MetaExchange.Core.Services;
public sealed class ExchangesLoader : IExchangesLoader
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    /// <inheritdoc />
    public async Task<IReadOnlyList<Exchange>> LoadExchangesAsync(string folder, CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(folder))
        {
            throw new InvalidOperationException($"Orderbooks folder not found: {folder}");
        }

        var files = Directory.EnumerateFiles(folder, "*.json", SearchOption.TopDirectoryOnly);
        List<Exchange> exchanges = [];

        foreach (var file in files)
        {
            await using var fs = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read);
            var exchange = await JsonSerializer.DeserializeAsync<Exchange>(fs, _jsonOptions, cancellationToken)
                ?? throw new InvalidOperationException($"Invalid JSON in {Path.GetFileName(file)}");

            exchanges.Add(exchange);
        }

        return exchanges;
    }
}
