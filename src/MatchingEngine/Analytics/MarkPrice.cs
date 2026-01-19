using MatchingEngine.Domain;

namespace MatchingEngine.Analytics;

public sealed class MarkPrice
{
    private long _lastTradePriceTicks;
    private bool _hasLastTrade;

    public long CurrentMarkTicks { get; private set; }

    public void UpdateFromBook(long? bestBidTicks, long? bestAskTicks, long fallbackMidTicks)
    {
        if (bestBidTicks.HasValue && bestAskTicks.HasValue)
        {
            CurrentMarkTicks = (bestBidTicks.Value + bestAskTicks.Value) / 2;
            return;
        }

        if (_hasLastTrade)
        {
            CurrentMarkTicks = _lastTradePriceTicks;
            return;
        }

        CurrentMarkTicks = fallbackMidTicks;
    }

    public void OnTrade(Trade trade)
    {
        _lastTradePriceTicks = trade.PriceTicks;
        _hasLastTrade = true;
        CurrentMarkTicks = trade.PriceTicks;
    }
}
