using System.Threading;

namespace MatchingEngine.Util;

public static class IdGenerator
{
    private static long _nextId;

    public static long NextId()
    {
        return Interlocked.Increment(ref _nextId);
    }
}
