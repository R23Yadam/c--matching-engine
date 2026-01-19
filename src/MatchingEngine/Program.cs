using System.Globalization;
using MatchingEngine.Analytics;
using MatchingEngine.Domain;
using MatchingEngine.Engine;
using MatchingEngine.Strategy;
using MatchingEngine.Util;

namespace MatchingEngine;

public static class Program
{
    public static void Main()
    {
        RunScenario();
        Console.WriteLine();
        RunSimulation();
    }

    private static void RunScenario()
    {
        var orderBook = new OrderBook();
        var engine = new Engine.MatchingEngine(orderBook);
        var pnl = new PnLTracker();
        var latency = new LatencyTracker();
        var mark = new MarkPrice();
        var agentOrderIds = new HashSet<long>();

        var askOrder = new Order(IdGenerator.NextId(), Side.Sell, OrderType.Limit, 10100, 10, TimeSource.NowTicks());
        engine.Process(askOrder, TimeSource.NowTicks());

        var bidOrder = new Order(IdGenerator.NextId(), Side.Buy, OrderType.Limit, 10050, 5, TimeSource.NowTicks());
        engine.Process(bidOrder, TimeSource.NowTicks());

        var buyMarket = new Order(IdGenerator.NextId(), Side.Buy, OrderType.Market, 0, 7, TimeSource.NowTicks());
        agentOrderIds.Add(buyMarket.Id);
        var scenarioTrades = engine.Process(buyMarket, TimeSource.NowTicks());

        foreach (var trade in scenarioTrades)
        {
            latency.Record(trade.AcceptTs, trade.FillTs);
            mark.OnTrade(trade);
            if (agentOrderIds.Contains(trade.BuyOrderId))
            {
                pnl.ApplyFill(Side.Buy, trade.PriceTicks, trade.Qty);
            }
        }

        mark.UpdateFromBook(orderBook.BestBidPriceTicks(), orderBook.BestAskPriceTicks(), 10000);
        pnl.UpdateMark(mark.CurrentMarkTicks);

        Console.WriteLine("TRADES");
        PrintTrades(scenarioTrades);

        Console.WriteLine("BOOK");
        PrintBook(orderBook);

        Console.WriteLine("PNL");
        PrintPnL(pnl);

        Console.WriteLine("LATENCY (us)");
        PrintLatency(latency);
    }

    private static void RunSimulation()
    {
        var orderBook = new OrderBook();
        var engine = new Engine.MatchingEngine(orderBook);
        var pnl = new PnLTracker();
        var latency = new LatencyTracker();
        var mark = new MarkPrice();
        var maker = new ToyMarketMaker(qty: 2, halfSpreadTicks: 5);
        var runner = new StrategyRunner(engine, orderBook, maker, pnl, latency, mark);
        var rng = new Random(42);

        runner.Run(
            ticks: 2000,
            startingMidTicks: 10000,
            maxStepTicks: 3,
            rng: rng,
            externalOrders: snapshot => CreateExternalOrders(snapshot, rng));

        Console.WriteLine("SIMULATION");
        Console.WriteLine("TRADES");
        PrintTrades(runner.Trades.Take(10));
        Console.WriteLine($"count={runner.Trades.Count}");

        Console.WriteLine("BOOK");
        PrintBook(orderBook);

        Console.WriteLine("PNL");
        PrintPnL(pnl);

        Console.WriteLine("LATENCY (us)");
        PrintLatency(latency);
    }

    private static IEnumerable<Order> CreateExternalOrders(MarketSnapshot snapshot, Random rng)
    {
        if (snapshot.BestBidPriceTicks == null || snapshot.BestAskPriceTicks == null)
        {
            return Array.Empty<Order>();
        }

        var roll = rng.Next(0, 100);
        if (roll < 30)
        {
            return new[]
            {
                new Order(
                    IdGenerator.NextId(),
                    Side.Buy,
                    OrderType.Market,
                    0,
                    rng.Next(1, 4),
                    TimeSource.NowTicks())
            };
        }

        if (roll > 70)
        {
            return new[]
            {
                new Order(
                    IdGenerator.NextId(),
                    Side.Sell,
                    OrderType.Market,
                    0,
                    rng.Next(1, 4),
                    TimeSource.NowTicks())
            };
        }

        return Array.Empty<Order>();
    }

    private static void PrintTrades(IEnumerable<Trade> trades)
    {
        foreach (var trade in trades)
        {
            Console.WriteLine(
                $"t={trade.FillTs} buyId={trade.BuyOrderId} sellId={trade.SellOrderId} px={FormatPrice(trade.PriceTicks)} qty={trade.Qty}");
        }
    }

    private static void PrintBook(OrderBook orderBook)
    {
        var bestBid = orderBook.PeekBestBidLevel();
        var bestAsk = orderBook.PeekBestAskLevel();

        Console.WriteLine(bestBid == null
            ? "bestBid=NA"
            : $"bestBid={FormatPrice(bestBid.PriceTicks)} x {bestBid.TotalQty}");
        Console.WriteLine(bestAsk == null
            ? "bestAsk=NA"
            : $"bestAsk={FormatPrice(bestAsk.PriceTicks)} x {bestAsk.TotalQty}");
    }

    private static void PrintPnL(PnLTracker pnl)
    {
        Console.WriteLine(
            $"pos={FormatSigned(pnl.Position)} avg={FormatPrice(pnl.AvgCostTicks)} realized={FormatMoney(pnl.RealizedPnlTicks)} " +
            $"unrealized={FormatMoney(pnl.UnrealizedPnlTicks)} total={FormatMoney(pnl.TotalPnlTicks)}");
    }

    private static void PrintLatency(LatencyTracker latency)
    {
        var stats = latency.GetStats();
        Console.WriteLine($"count={stats.Count}");
        Console.WriteLine($"avg={stats.AvgUs}");
        Console.WriteLine($"p50={stats.P50Us}");
        Console.WriteLine($"p95={stats.P95Us}");
    }

    private static string FormatSigned(int value)
    {
        return value >= 0 ? $"+{value}" : value.ToString(CultureInfo.InvariantCulture);
    }

    private static string FormatPrice(long priceTicks)
    {
        return (priceTicks / 100m).ToString("0.00", CultureInfo.InvariantCulture);
    }

    private static string FormatMoney(long pnlTicks)
    {
        return (pnlTicks / 100m).ToString("0.00", CultureInfo.InvariantCulture);
    }
}
