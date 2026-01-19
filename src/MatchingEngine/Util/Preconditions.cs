namespace MatchingEngine.Util;

public static class Preconditions
{
    public static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new ArgumentException(message);
        }
    }
}
