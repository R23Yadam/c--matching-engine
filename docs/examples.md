# Examples

## Hardcoded scenario
1. Add ask: sell 10 @ 101.00
2. Add bid: buy 5 @ 100.50
3. Send buy market 7 → trade 7 at 101.00

Example output:
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
