using MatchingEngine.Util;

namespace MatchingEngine.Domain;

public sealed class Order
{
    public long Id { get; }
    public Side Side { get; }
    public OrderType Type { get; }
    public long PriceTicks { get; }
    public int Qty { get; set; }
    public long Timestamp { get; }

    public Order(long id, Side side, OrderType type, long priceTicks, int qty, long timestamp)
    {
        Preconditions.Require(id > 0, "id must be positive");
        Preconditions.Require(qty > 0, "qty must be positive");

        if (type == OrderType.Limit)
        {
            Preconditions.Require(priceTicks > 0, "limit price must be positive");
        }
        else
        {
            Preconditions.Require(priceTicks >= 0, "market price must be non-negative");
        }

        Id = id;
        Side = side;
        Type = type;
        PriceTicks = priceTicks;
        Qty = qty;
        Timestamp = timestamp;
    }
}
