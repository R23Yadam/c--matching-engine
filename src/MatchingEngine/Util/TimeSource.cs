using System.Diagnostics;

namespace MatchingEngine.Util;

public static class TimeSource
{
    public static long NowTicks()
    {
        return Stopwatch.GetTimestamp();
    }

    public static long Frequency => Stopwatch.Frequency;
}
