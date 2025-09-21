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
2. Sequence Diagram
	- BUY Flow
	- SELL Flow
3. Class Diagram 
