using MatchingEngine.Analytics;
using MatchingEngine.Domain;
using Xunit;

namespace MatchingEngine.Tests;

public class PnLTests
{
    [Fact]
    public void LongThenSellRealizesPnl()
    {
        var pnl = new PnLTracker();

        pnl.ApplyFill(Side.Buy, 10000, 10);
        pnl.ApplyFill(Side.Sell, 10100, 4);

        Assert.Equal(6, pnl.Position);
        Assert.Equal(10000, pnl.AvgCostTicks);
        Assert.Equal(400, pnl.RealizedPnlTicks);
    }

    [Fact]
    public void ShortThenBuyFlipsPosition()
    {
        var pnl = new PnLTracker();

        pnl.ApplyFill(Side.Sell, 10000, 5);
        pnl.ApplyFill(Side.Buy, 9900, 8);

        Assert.Equal(3, pnl.Position);
        Assert.Equal(9900, pnl.AvgCostTicks);
        Assert.Equal(500, pnl.RealizedPnlTicks);
    }

    [Fact]
    public void LongThenSellFlipsToShort()
    {
        var pnl = new PnLTracker();

        pnl.ApplyFill(Side.Buy, 10000, 5);
        pnl.ApplyFill(Side.Sell, 10100, 8);

        Assert.Equal(-3, pnl.Position);
        Assert.Equal(10100, pnl.AvgCostTicks);
        Assert.Equal(500, pnl.RealizedPnlTicks);
    }
}
