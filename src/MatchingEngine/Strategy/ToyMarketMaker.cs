using MatchingEngine.Domain;
using MatchingEngine.Util;

namespace MatchingEngine.Strategy;

public sealed class ToyMarketMaker : IStrategy
{
    private readonly int _qty;
    private readonly long _halfSpreadTicks;

    public ToyMarketMaker(int qty, long halfSpreadTicks)
    {
        _qty = qty;
        _halfSpreadTicks = halfSpreadTicks;
    }

    public IEnumerable<Order> OnTick(MarketSnapshot snapshot)
    {
        if (snapshot.MarkPriceTicks <= 0)
        {
            yield break;
        }

        var bidPrice = Math.Max(1, snapshot.MarkPriceTicks - _halfSpreadTicks);
        var askPrice = snapshot.MarkPriceTicks + _halfSpreadTicks;
        var ts = TimeSource.NowTicks();

        yield return new Order(
            IdGenerator.NextId(),
            Side.Buy,
            OrderType.Limit,
            bidPrice,
            _qty,
            ts);

        yield return new Order(
            IdGenerator.NextId(),
            Side.Sell,
            OrderType.Limit,
            askPrice,
            _qty,
            ts);
    }
}
