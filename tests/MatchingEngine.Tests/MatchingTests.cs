using MatchingEngine.Domain;
using MatchingEngine.Engine;
using MatchingEngine.Util;
using Xunit;

namespace MatchingEngine.Tests;

public class MatchingTests
{
    [Fact]
    public void FIFOAtSamePriceFillsInOrder()
    {
        var book = new OrderBook();
        var engine = new MatchingEngine.Engine.MatchingEngine(book);

        var ask1 = new Order(IdGenerator.NextId(), Side.Sell, OrderType.Limit, 10100, 5, 1);
        var ask2 = new Order(IdGenerator.NextId(), Side.Sell, OrderType.Limit, 10100, 5, 2);
        engine.Process(ask1, 10);
        engine.Process(ask2, 11);

        var buyMarket = new Order(IdGenerator.NextId(), Side.Buy, OrderType.Market, 0, 7, 3);
        var trades = engine.Process(buyMarket, 12);

        Assert.Equal(2, trades.Count);
        Assert.Equal(ask1.Id, trades[0].SellOrderId);
        Assert.Equal(5, trades[0].Qty);
        Assert.Equal(ask2.Id, trades[1].SellOrderId);
        Assert.Equal(2, trades[1].Qty);
    }

    [Fact]
    public void LimitBuyCrossesBestAsk()
    {
        var book = new OrderBook();
        var engine = new MatchingEngine.Engine.MatchingEngine(book);

        var ask = new Order(IdGenerator.NextId(), Side.Sell, OrderType.Limit, 10100, 3, 1);
        engine.Process(ask, 10);

        var buy = new Order(IdGenerator.NextId(), Side.Buy, OrderType.Limit, 10150, 3, 2);
        var trades = engine.Process(buy, 11);

        Assert.Single(trades);
        Assert.Equal(ask.PriceTicks, trades[0].PriceTicks);
        Assert.Equal(3, trades[0].Qty);
    }

    [Fact]
    public void PartialFillLeavesRemainderOnBook()
    {
        var book = new OrderBook();
        var engine = new MatchingEngine.Engine.MatchingEngine(book);

        var ask = new Order(IdGenerator.NextId(), Side.Sell, OrderType.Limit, 10100, 5, 1);
        engine.Process(ask, 10);

        var buy = new Order(IdGenerator.NextId(), Side.Buy, OrderType.Limit, 10150, 10, 2);
        var trades = engine.Process(buy, 11);

        Assert.Single(trades);
        Assert.Equal(5, trades[0].Qty);

        var bestBid = book.PeekBestBidLevel();
        Assert.NotNull(bestBid);
        Assert.Equal(10150, bestBid!.PriceTicks);
        Assert.Equal(5, bestBid.TotalQty);
    }

    [Fact]
    public void MarketOrderOnEmptyBookProducesNoTrades()
    {
        var book = new OrderBook();
        var engine = new MatchingEngine.Engine.MatchingEngine(book);

        var buyMarket = new Order(IdGenerator.NextId(), Side.Buy, OrderType.Market, 0, 5, 1);
        var buyTrades = engine.Process(buyMarket, 2);

        Assert.Empty(buyTrades);
        Assert.Null(book.BestBidPriceTicks());
        Assert.Null(book.BestAskPriceTicks());

        var sellMarket = new Order(IdGenerator.NextId(), Side.Sell, OrderType.Market, 0, 5, 3);
        var sellTrades = engine.Process(sellMarket, 4);

        Assert.Empty(sellTrades);
        Assert.Null(book.BestBidPriceTicks());
        Assert.Null(book.BestAskPriceTicks());
    }

    [Fact]
    public void MarketSellCrossesBestBid()
    {
        var book = new OrderBook();
        var engine = new MatchingEngine.Engine.MatchingEngine(book);

        var bid = new Order(IdGenerator.NextId(), Side.Buy, OrderType.Limit, 10000, 5, 1);
        engine.Process(bid, 2);

        var sellMarket = new Order(IdGenerator.NextId(), Side.Sell, OrderType.Market, 0, 3, 3);
        var trades = engine.Process(sellMarket, 4);

        Assert.Single(trades);
        Assert.Equal(bid.Id, trades[0].BuyOrderId);
        Assert.Equal(sellMarket.Id, trades[0].SellOrderId);
        Assert.Equal(10000, trades[0].PriceTicks);
        Assert.Equal(3, trades[0].Qty);

        var bestBid = book.PeekBestBidLevel();
        Assert.NotNull(bestBid);
        Assert.Equal(10000, bestBid!.PriceTicks);
        Assert.Equal(2, bestBid.TotalQty);
    }

    [Fact]
    public void LimitOrderThatDoesNotCrossRestsOnBook()
    {
        var book = new OrderBook();
        var engine = new MatchingEngine.Engine.MatchingEngine(book);

        var ask = new Order(IdGenerator.NextId(), Side.Sell, OrderType.Limit, 10100, 4, 1);
        engine.Process(ask, 2);

        var buy = new Order(IdGenerator.NextId(), Side.Buy, OrderType.Limit, 10000, 5, 3);
        var trades = engine.Process(buy, 4);

        Assert.Empty(trades);
        Assert.Equal(10000, book.BestBidPriceTicks());
        Assert.Equal(10100, book.BestAskPriceTicks());

        var bestBid = book.PeekBestBidLevel();
        var bestAsk = book.PeekBestAskLevel();
        Assert.NotNull(bestBid);
        Assert.NotNull(bestAsk);
        Assert.Equal(5, bestBid!.TotalQty);
        Assert.Equal(4, bestAsk!.TotalQty);
    }

    [Fact]
    public void MultiLevelWalkMatchesBestPriceFirst()
    {
        var book = new OrderBook();
        var engine = new MatchingEngine.Engine.MatchingEngine(book);

        var ask1 = new Order(IdGenerator.NextId(), Side.Sell, OrderType.Limit, 10100, 3, 1);
        var ask2 = new Order(IdGenerator.NextId(), Side.Sell, OrderType.Limit, 10200, 5, 2);
        engine.Process(ask1, 3);
        engine.Process(ask2, 4);

        var buy = new Order(IdGenerator.NextId(), Side.Buy, OrderType.Limit, 10200, 6, 5);
        var trades = engine.Process(buy, 6);

        Assert.Equal(2, trades.Count);
        Assert.Equal(ask1.Id, trades[0].SellOrderId);
        Assert.Equal(10100, trades[0].PriceTicks);
        Assert.Equal(3, trades[0].Qty);
        Assert.Equal(ask2.Id, trades[1].SellOrderId);
        Assert.Equal(10200, trades[1].PriceTicks);
        Assert.Equal(3, trades[1].Qty);

        Assert.Null(book.BestBidPriceTicks());
        var bestAsk = book.PeekBestAskLevel();
        Assert.NotNull(bestAsk);
        Assert.Equal(10200, bestAsk!.PriceTicks);
        Assert.Equal(2, bestAsk.TotalQty);
    }
}
