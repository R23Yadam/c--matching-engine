using MatchingEngine.Util;

namespace MatchingEngine.Analytics;

public sealed class LatencyTracker
{
    private readonly List<long> _latenciesUs = new();

    public void Record(long acceptTs, long fillTs)
    {
        var deltaTicks = fillTs - acceptTs;
        if (deltaTicks < 0)
        {
            return;
        }

        var latencyUs = deltaTicks * 1_000_000 / TimeSource.Frequency;
        _latenciesUs.Add(latencyUs);
    }

    public LatencyStats GetStats()
    {
        if (_latenciesUs.Count == 0)
        {
            return new LatencyStats(0, 0, 0, 0);
        }

        var latencies = _latenciesUs.ToArray();
        Array.Sort(latencies);

        long sum = 0;
        for (var i = 0; i < latencies.Length; i++)
        {
            sum += latencies[i];
        }

        var count = latencies.Length;
        var avg = sum / count;
        var p50Index = (int)((count - 1) * 0.50);
        var p95Index = (int)((count - 1) * 0.95);

        return new LatencyStats(count, avg, latencies[p50Index], latencies[p95Index]);
    }
}

public readonly record struct LatencyStats(int Count, long AvgUs, long P50Us, long P95Us);
