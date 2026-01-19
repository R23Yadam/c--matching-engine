using MatchingEngine.Domain;
using MatchingEngine.Engine;
using MatchingEngine.Util;
using Xunit;

namespace MatchingEngine.Tests;

public class OrderBookTests
{
    [Fact]
    public void BestBidAskReturnsTopOfBook()
    {
        var book = new OrderBook();

        book.AddLimit(new Order(IdGenerator.NextId(), Side.Buy, OrderType.Limit, 10000, 5, 1));
        book.AddLimit(new Order(IdGenerator.NextId(), Side.Sell, OrderType.Limit, 10100, 7, 2));

        Assert.Equal(10000, book.BestBidPriceTicks());
        Assert.Equal(10100, book.BestAskPriceTicks());
    }

    [Fact]
    public void DequeueRemovesEmptyLevelAndRevealsNextBest()
    {
        var book = new OrderBook();

        var topBid = new Order(IdGenerator.NextId(), Side.Buy, OrderType.Limit, 10100, 3, 1);
        var nextBid = new Order(IdGenerator.NextId(), Side.Buy, OrderType.Limit, 10000, 4, 2);
        book.AddLimit(topBid);
        book.AddLimit(nextBid);

        var removed = book.DequeueAt(10100, Side.Buy);

        Assert.NotNull(removed);
        Assert.Equal(topBid.Id, removed!.Id);
        Assert.Equal(10000, book.BestBidPriceTicks());
        Assert.Equal(10000, book.PeekBestBidLevel()!.PriceTicks);
    }
}
