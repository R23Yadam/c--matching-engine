using MatchingEngine.Analytics;
using MatchingEngine.Util;
using Xunit;

namespace MatchingEngine.Tests;

public class LatencyTests
{
    [Fact]
    public void TicksConvertToMicroseconds()
    {
        var tracker = new LatencyTracker();
        var acceptTs = 1_000L;
        var fillTs = acceptTs + TimeSource.Frequency;

        tracker.Record(acceptTs, fillTs);
        var stats = tracker.GetStats();

        Assert.Equal(1, stats.Count);
        Assert.Equal(1_000_000, stats.AvgUs);
        Assert.Equal(1_000_000, stats.P50Us);
        Assert.Equal(1_000_000, stats.P95Us);
    }

    [Fact]
    public void StatsIgnoreNegativeAndUsePercentileIndices()
    {
        var tracker = new LatencyTracker();
        var acceptTs = 10_000L;

        tracker.Record(acceptTs, acceptTs + TimeSource.Frequency * 5);
        tracker.Record(acceptTs, acceptTs + TimeSource.Frequency * 1);
        tracker.Record(acceptTs, acceptTs + TimeSource.Frequency * 3);
        tracker.Record(acceptTs, acceptTs + TimeSource.Frequency * 2);
        tracker.Record(acceptTs, acceptTs + TimeSource.Frequency * 4);
        tracker.Record(10, 5);

        var stats = tracker.GetStats();

        Assert.Equal(5, stats.Count);
        Assert.Equal(3_000_000, stats.AvgUs);
        Assert.Equal(3_000_000, stats.P50Us);
        Assert.Equal(4_000_000, stats.P95Us);
    }
}
