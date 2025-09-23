using MetaExchange.Core.Enums;
using MetaExchange.Core.Models;

namespace MetaExchange.Core.Abstractions;

public interface IExecutionPlanner
{
    /// <summary>
    /// Generates an execution plan to optionally buy or sell BTC across multiple exchanges.
    /// </summary>
    /// <param name="exchanges">The list of exchange order books and balances.</param>
    /// <param name="tradeType">The type of trade: BUY or SELL.</param>
    /// <param name="amountBtc">The total amount of BTC to buy or sell.</param>
    /// <returns>An execution plan containing orders and post-trade balances.</returns>
    ExecutionPlan Execute(IReadOnlyList<Exchange> exchanges, TradeType tradeType, decimal amountBtc);
}
