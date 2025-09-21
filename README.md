# MetaExchange

This solution calculates the best way to execute a BTC buy or sell order across multiple exchanges. 
In finance, this type of system is often called a Smart Order Router (SOR). 
This implementation focuses on the core logic combining order books, respecting balances, and producing an execution plan.

This project includes:
- A **Console App** for batch execution (`MetaExchange.Cli`)
- A **Core Library** containing the execution algorithm (`MetaExchange.Core`)

## Features
- Reads multiple exchange **order book JSON files**.
- Optimizes trades:
  - **BUY** at lowest prices first.
  - **SELL** at highest prices first.
- Respects per-exchange limits:
  - EUR balance for buys,
  - BTC balance for sells.
- Outputs a detailed **execution plan**:
  - Orders to place per exchange,
  - Weighted average price,
  - Updated post-trade balances.

## Folder Structure
MetaExchange/
|
|--- orderbooks/ # Input JSON files
| |--- exchange-01.json
| |--- exchange-02.json
|
|--- src/
| |--- MetaExchange.CLI/ # Console App
| |--- MetaExchange.Core/ # Core logic and models

## Example Input JSON Files
Example: `orderbooks/exchange-01.json`
```json
{
	"Id": "Binance",
	"AvailableFunds": {
		"Crypto": 0.5,
		"Euro": 10000.0
	},
	"OrderBook": {
		"Bids": [
			{
				"Order": {
					"Price": 25000.00,
					"Amount": 0.4
				}
			}
		],
		"Asks": [
			{
				"Order": {
					"Price": 25100.00,
					"Amount": 0.2
				}
			}
		]
	}
}
```

## How to Run
### Console App
```bash
cd src/MetaExchange.CLI

# Output in Console
dotnet run ../../orderbooks BUY 1.0

# Output in File
dotnet run ../../orderbooks BUY 1.0 result.json
```

#### Arguments
| Argument					| Description							|
|:--------------------------|:--------------------------------------|
| `<orderbooksFolder>`		| Folder containing JSON files.			|
| `<BUY>` or `<SELL>`		| Type of order (Buy or Sell).			|
| `<amountBtc>`				| Total BTC to trade.					|
| `[outputFile]`			| JSON file to save output (*optional*)	|


## UML Diagrams
1. Architecture Overview
This diagram shows how the Console App use the core logic.
```mermaid
flowchart LR
	subgraph CLI[MetaExchange.CLI]
		CLIProgram[Program.cs]
	end

	subgraph Core[MetaExchange.Core]
		Planner[ExecutionPlanner]
		Loader[ExchangesLoader]
		Models["(Models)"]
	end

	CLIProgram --> Loader
	CLIProgram --> Planner

	Planner --> Models
	Loader --> Models
```

2. Sequence Diagram

	- BUY Flow
	Shows what happens when a BUY order processed.
	```mermaid
	sequenceDiagram
		autonumber
		participant User
		participant CLI as Console
		participant Loader as ExchangeLoader
		participant Planner as ExecutionPlanner
 
		User->>CLI: Request BUY 1.0
		CLI->>Loader: Load JSON order books
		Loader-->>CLI: List of Exchanges
		CLI->>Planner: Execute(exchanges, BUY, 1.0)
		Planner->>Planner: Sort asks by price ASC (cheapest first)
		Planner->>Planner: Allocate BTC by EUR balance
		Planner-->>CLI: ExecutionPlan
		CLI-->>User: Print JSON plan
	```

	- SELL Flow
	Shows what happens when a SELL order processed.
	```mermaid
	sequenceDiagram
		autonumber
		participant User
		participant CLI as Console
		participant Loader as ExchangeLoader
		participant Planner as ExecutionPlanner
 
		User->>CLI: Request SELL 1.0
		CLI->>Loader: Load JSON order books
		Loader-->>CLI: List of Exchanges
		CLI->>Planner: Execute(exchanges, SELL, 1.0)

		Planner->>Planner: Sort bids by price DESC (highest first)

		loop For each bid level
			Planner->>Planner: cappedQuantity = min(remaining, levelSize, BTC balance)
			alt cappedQuantity > 0
				Planner->>Planner: lineNotional = price * cappedQuantity
				Planner->>Planner: Update balances:<br/>btc -= cappedQuantity<br/>eur += lineNotional
				Planner-->>Planner: Add ExecutionOrder
				Planner->>Planner: remining -= cappedQuantity<br/>filled += cappedQuantity<br/>notional += lineNotional
			else
				Planner->>Planner: Skip empty level
			end
			opt remaining == 0
				Planner->>Planner: Break loop
			end
		end

		Planner-->>CLI: ExecutionPlan
		CLI-->>User: Print JSON plan
	```

3. Class Diagram 
```mermaid
classDiagram
	class ExecutionPlanner {
		+Execute(exchanges, side, amountBtc): ExecutionPlan
		-BuildPriceLevels(exchanges, side): IEnumerable<PriceLevel>
		-SortPriceLevels(priceLevels, side): IEnumerable<PriceLevel>
	}

	class ExchangeLoader {
		+LoadExchanges(folder): IReadOnlyList<Exchange>
	}

	class Exchange {
		+Id: string
		+AvailableFunds: AvailableFunds
		+OrderBook: OrderBook
	}

	class AvailableFunds {
		+Crypto: decimal
		+Euro: decimal
	}

	class OrderBook {
		+Bids: List<WrappedOrder>
		+Asks: List<WrappedOrder>
	}

	class WrappedOrder {
		+Order: Order
	}

	class Order {
		+Id: string
		+Time: DateTime
		+Type: string
		+Kind: string
		+Amount: decimal
		+Price: decimal
	}

	class PriceLevel {
		+ExchangeId: string
		+Price: decimal
		+Size: decimal
	}

	class ExecutionPlan {
		+Side: string
		+RequestedAmountBtc: decimal
		+FilledAmountBtc: decimal
		+ShortfallBtc: decimal
		+WeightedAveragePrice: decimal
		+TotalNotionalEur: decimal
		+Orders: List<ExecutionOrder>
		+PostTradeBalances: List<PostTradeBalace>
	}

	class ExecutionOrder {
		+ExchangeId: string
		+Price: decimal
		+QuantityBtc: decimal
		+NotionalEur: decimal
	}

	class PostTradeBalance {
		+ExchangeId: string
		+Euro: decimal
		+Crypto: decimal
	}

	ExecutionPlanner --> ExecutionPlan
	ExecutionPlanner --> PriceLevel
	ExchangeLoader --> Exchange
	Exchange --> AvailableFunds
	Exchange --> OrderBook
	OrderBook --> WrappedOrder
	WrappedOrder --> Order
	ExecutionPlan --> ExecutionOrder
	ExecutionPlan --> PostTradeBalance
```
