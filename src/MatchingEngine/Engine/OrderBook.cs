using MatchingEngine.Domain;

namespace MatchingEngine.Engine;

public sealed class OrderBook
{
    private readonly SortedDictionary<long, PriceLevel> _bids;
    private readonly SortedDictionary<long, PriceLevel> _asks;

    public OrderBook()
    {
        _bids = new SortedDictionary<long, PriceLevel>(Comparer<long>.Create((a, b) => b.CompareTo(a)));
        _asks = new SortedDictionary<long, PriceLevel>();
    }

    public long? BestBidPriceTicks()
    {
        return TryGetBestPrice(_bids);
    }

    public long? BestAskPriceTicks()
    {
        return TryGetBestPrice(_asks);
    }

    public PriceLevel? PeekBestBidLevel()
    {
        return TryGetBestLevel(_bids);
    }

    public PriceLevel? PeekBestAskLevel()
    {
        return TryGetBestLevel(_asks);
    }

    public void AddLimit(Order order)
    {
        var levels = order.Side == Side.Buy ? _bids : _asks;
        if (!levels.TryGetValue(order.PriceTicks, out var level))
        {
            level = new PriceLevel(order.PriceTicks);
            levels[order.PriceTicks] = level;
        }

        level.Enqueue(order);
    }

    public Order? DequeueAt(long priceTicks, Side side)
    {
        var levels = side == Side.Buy ? _bids : _asks;
        if (!levels.TryGetValue(priceTicks, out var level))
        {
            return null;
        }

        var order = level.Dequeue();
        if (!level.HasOrders)
        {
            levels.Remove(priceTicks);
        }

        return order;
    }

    private static long? TryGetBestPrice(SortedDictionary<long, PriceLevel> levels)
    {
        foreach (var kvp in levels)
        {
            return kvp.Key;
        }

        return null;
    }

    private static PriceLevel? TryGetBestLevel(SortedDictionary<long, PriceLevel> levels)
    {
        foreach (var kvp in levels)
        {
            return kvp.Value;
        }

        return null;
    }
}
