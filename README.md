
https://github.com/user-attachments/assets/fc98534c-4a3d-4ca6-83b1-f14758377e5d
https://github.com/R23Yadam/c--matching-engine/actions/workflows/ci.yml/badge.svg

# csharp-matching-engine
Deterministic C# matching engine with clear module boundaries and analytics.


https://github.com/user-attachments/assets/f2a83b4d-9cef-4672-8c9e-26a4351487ed


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
