using MatchingEngine.Analytics;
using MatchingEngine.Domain;
using Xunit;

namespace MatchingEngine.Tests;

public class MarkPriceTests
{
    [Fact]
    public void UpdateFromBookUsesMidLastTradeOrFallback()
    {
        var mark = new MarkPrice();

        mark.UpdateFromBook(10000, 10200, 9900);
        Assert.Equal(10100, mark.CurrentMarkTicks);

        var trade = new Trade(1, 2, 10300, 1, 10, 11);
        mark.OnTrade(trade);
        Assert.Equal(10300, mark.CurrentMarkTicks);

        mark.UpdateFromBook(10000, null, 9900);
        Assert.Equal(10300, mark.CurrentMarkTicks);

        var fresh = new MarkPrice();
        fresh.UpdateFromBook(null, null, 9800);
        Assert.Equal(9800, fresh.CurrentMarkTicks);
    }
}
