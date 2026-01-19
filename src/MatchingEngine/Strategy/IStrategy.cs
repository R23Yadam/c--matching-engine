using MatchingEngine.Domain;

namespace MatchingEngine.Strategy;

public interface IStrategy
{
    IEnumerable<Order> OnTick(MarketSnapshot snapshot);
}

public readonly struct MarketSnapshot
{
    public MarketSnapshot(long? bestBidPriceTicks, long? bestAskPriceTicks, long markPriceTicks, long timestamp)
    {
        BestBidPriceTicks = bestBidPriceTicks;
        BestAskPriceTicks = bestAskPriceTicks;
        MarkPriceTicks = markPriceTicks;
        Timestamp = timestamp;
    }

    public long? BestBidPriceTicks { get; }
    public long? BestAskPriceTicks { get; }
    public long MarkPriceTicks { get; }
    public long Timestamp { get; }
}
