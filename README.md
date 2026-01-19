# csharp-matching-engine
Deterministic C# matching engine with clear module boundaries and analytics.

## Features
- Price-time priority (FIFO).
- Market + limit orders.
- Deterministic order stream → deterministic trades.
- PnL tracking (realized/unrealized).
- Execution latency measurement (accept→fill).

## Design choices
- Price stored as integer ticks to avoid floating point.
- Single-threaded for determinism and clarity.
- Minimal APIs and module boundaries.

Deterministic processing: same input order stream produces the same trades.

## How to run
- Demo: `dotnet run --project src/MatchingEngine/MatchingEngine.csproj`
- Tests: `dotnet test`

## Example output
TRADES
t=... buyId=... sellId=... px=101.00 qty=7

BOOK
bestBid=100.50 x 5
bestAsk=101.00 x 3

PNL
pos=+7 avg=101.00 realized=0.00 unrealized=...

LATENCY (us)
count=1
avg=...
p50=...
p95=...

## Notes / limitations
- Single symbol, in-memory book.
- No order cancellations or amendments.
- No persistence or networking.

## Roadmap
- Order cancel/replace.
- Snapshot/restore.
