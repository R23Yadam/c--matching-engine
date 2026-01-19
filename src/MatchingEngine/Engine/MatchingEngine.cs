using MatchingEngine.Domain;
using MatchingEngine.Util;

namespace MatchingEngine.Engine;

public sealed class MatchingEngine
{
    private readonly OrderBook _orderBook;

    public MatchingEngine(OrderBook orderBook)
    {
        _orderBook = orderBook;
    }

    public IReadOnlyList<Trade> Process(Order order, long acceptTs)
    {
        Preconditions.Require(order.Qty > 0, "order qty must be positive");

        var trades = new List<Trade>();

        if (order.Type == OrderType.Market)
        {
            MatchMarket(order, acceptTs, trades);
        }
        else
        {
            MatchLimit(order, acceptTs, trades);
        }

        return trades;
    }

    private void MatchMarket(Order order, long acceptTs, List<Trade> trades)
    {
        if (order.Side == Side.Buy)
        {
            MatchAgainstAsks(order, acceptTs, trades, isMarket: true);
            return;
        }

        MatchAgainstBids(order, acceptTs, trades, isMarket: true);
    }

    private void MatchLimit(Order order, long acceptTs, List<Trade> trades)
    {
        if (order.Side == Side.Buy)
        {
            MatchAgainstAsks(order, acceptTs, trades, isMarket: false);
            if (order.Qty > 0)
            {
                _orderBook.AddLimit(order);
            }

            return;
        }

        MatchAgainstBids(order, acceptTs, trades, isMarket: false);
        if (order.Qty > 0)
        {
            _orderBook.AddLimit(order);
        }
    }

    private void MatchAgainstAsks(Order order, long acceptTs, List<Trade> trades, bool isMarket)
    {
        while (order.Qty > 0)
        {
            var bestAskLevel = _orderBook.PeekBestAskLevel();
            if (bestAskLevel == null)
            {
                return;
            }

            if (!isMarket && order.PriceTicks < bestAskLevel.PriceTicks)
            {
                return;
            }

            FillAtLevel(order, bestAskLevel, acceptTs, trades, Side.Sell);
        }
    }

    private void MatchAgainstBids(Order order, long acceptTs, List<Trade> trades, bool isMarket)
    {
        while (order.Qty > 0)
        {
            var bestBidLevel = _orderBook.PeekBestBidLevel();
            if (bestBidLevel == null)
            {
                return;
            }

            if (!isMarket && order.PriceTicks > bestBidLevel.PriceTicks)
            {
                return;
            }

            FillAtLevel(order, bestBidLevel, acceptTs, trades, Side.Buy);
        }
    }

    private void FillAtLevel(Order incoming, PriceLevel level, long acceptTs, List<Trade> trades, Side restingSide)
    {
        var resting = level.Peek();
        var fillQty = Math.Min(incoming.Qty, resting.Qty);
        var fillTs = TimeSource.NowTicks();
        var tradePrice = level.PriceTicks;

        if (restingSide == Side.Sell)
        {
            trades.Add(new Trade(incoming.Id, resting.Id, tradePrice, fillQty, acceptTs, fillTs));
        }
        else
        {
            trades.Add(new Trade(resting.Id, incoming.Id, tradePrice, fillQty, acceptTs, fillTs));
        }

        incoming.Qty -= fillQty;
        resting.Qty -= fillQty;
        level.ReduceTopQty(fillQty);

        if (resting.Qty == 0)
        {
            _orderBook.DequeueAt(level.PriceTicks, restingSide);
        }
    }
}
