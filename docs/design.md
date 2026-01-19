# Design

## Data structures
- Bids and asks stored in sorted price maps.
- FIFO queue per price level for resting orders.
- Trades are immutable records.

## Invariants
- Orders at the same price are matched FIFO.
- Trade price equals resting order price.
- Prices are integer ticks (cents).

## Matching rules
- Buy crosses if buyPrice >= bestAsk.
- Sell crosses if sellPrice <= bestBid.
- Market orders walk the book until filled or empty.
- Limit orders cross immediately; remainder rests.

## Complexity
- Best price lookup: O(log P) to insert, O(1) to peek (first key).
- FIFO queue per price level: enqueue/dequeue O(1).
- ProcessOrder = O(k log P) where k = number of price levels crossed / fills.
