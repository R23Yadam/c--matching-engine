using MatchingEngine.Analytics;
using MatchingEngine.Domain;
using MatchingEngine.Engine;
using MatchingEngine.Util;

namespace MatchingEngine.Strategy;

public sealed class StrategyRunner
{
    private readonly MatchingEngine.Engine.MatchingEngine _engine;
    private readonly OrderBook _orderBook;
    private readonly IStrategy _strategy;
    private readonly PnLTracker _pnl;
    private readonly LatencyTracker _latency;
    private readonly MarkPrice _markPrice;
    private readonly HashSet<long> _agentOrderIds = new();
    private readonly List<Trade> _trades = new();

    public StrategyRunner(
        MatchingEngine.Engine.MatchingEngine engine,
        OrderBook orderBook,
        IStrategy strategy,
        PnLTracker pnl,
        LatencyTracker latency,
        MarkPrice markPrice)
    {
        _engine = engine;
        _orderBook = orderBook;
        _strategy = strategy;
        _pnl = pnl;
        _latency = latency;
        _markPrice = markPrice;
    }

    public IReadOnlyList<Trade> Trades => _trades;

    public void Run(
        int ticks,
        long startingMidTicks,
        int maxStepTicks,
        Random rng,
        Func<MarketSnapshot, IEnumerable<Order>>? externalOrders = null)
    {
        var midTicks = startingMidTicks;

        for (var i = 0; i < ticks; i++)
        {
            var step = rng.Next(-maxStepTicks, maxStepTicks + 1);
            midTicks = Math.Max(1, midTicks + step);

            var snapshot = new MarketSnapshot(
                _orderBook.BestBidPriceTicks(),
                _orderBook.BestAskPriceTicks(),
                midTicks,
                TimeSource.NowTicks());

            foreach (var order in _strategy.OnTick(snapshot))
            {
                _agentOrderIds.Add(order.Id);
                var acceptTs = TimeSource.NowTicks();
                var trades = _engine.Process(order, acceptTs);
                foreach (var trade in trades)
                {
                    _trades.Add(trade);
                    _latency.Record(trade.AcceptTs, trade.FillTs);
                    _markPrice.OnTrade(trade);
                    ApplyAgentPnL(trade);
                }
            }

            if (externalOrders != null)
            {
                foreach (var order in externalOrders(snapshot))
                {
                    var acceptTs = TimeSource.NowTicks();
                    var trades = _engine.Process(order, acceptTs);
                    foreach (var trade in trades)
                    {
                        _trades.Add(trade);
                        _latency.Record(trade.AcceptTs, trade.FillTs);
                        _markPrice.OnTrade(trade);
                        ApplyAgentPnL(trade);
                    }
                }
            }

            _markPrice.UpdateFromBook(
                _orderBook.BestBidPriceTicks(),
                _orderBook.BestAskPriceTicks(),
                midTicks);
            _pnl.UpdateMark(_markPrice.CurrentMarkTicks);
        }
    }

    private void ApplyAgentPnL(Trade trade)
    {
        var isBuyAgent = _agentOrderIds.Contains(trade.BuyOrderId);
        var isSellAgent = _agentOrderIds.Contains(trade.SellOrderId);

        if (isBuyAgent && !isSellAgent)
        {
            _pnl.ApplyFill(Side.Buy, trade.PriceTicks, trade.Qty);
        }
        else if (isSellAgent && !isBuyAgent)
        {
            _pnl.ApplyFill(Side.Sell, trade.PriceTicks, trade.Qty);
        }
    }
}
