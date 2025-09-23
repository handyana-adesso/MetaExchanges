using MetaExchange.Core.Models;

namespace MetaExchange.Core.Abstractions;

public interface IExchangesLoader
{
    /// <summary>
    /// Loads exchange data (order books and balances) from JSON files asynchronously.
    /// </summary>
    /// <param name="folder">Path to the folder containing exchange JSON files.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A read-only list of exchagnes with their data.</returns>
    Task<IReadOnlyList<Exchange>> LoadExchangesAsync(string folder, CancellationToken cancellationToken = default);
}
