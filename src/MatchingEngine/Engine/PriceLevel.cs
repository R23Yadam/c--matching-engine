using MatchingEngine.Domain;
using MatchingEngine.Util;

namespace MatchingEngine.Engine;

public sealed class PriceLevel
{
    private readonly Queue<Order> _orders = new();
    private int _totalQty;

    public PriceLevel(long priceTicks)
    {
        Preconditions.Require(priceTicks > 0, "price ticks must be positive");
        PriceTicks = priceTicks;
    }

    public long PriceTicks { get; }
    public int TotalQty => _totalQty;
    public int Count => _orders.Count;
    public bool HasOrders => _orders.Count > 0;

    public void Enqueue(Order order)
    {
        _orders.Enqueue(order);
        _totalQty += order.Qty;
    }

    public Order Peek()
    {
        return _orders.Peek();
    }

    public Order Dequeue()
    {
        var order = _orders.Dequeue();
        _totalQty -= order.Qty;
        return order;
    }

    public void ReduceTopQty(int fillQty)
    {
        Preconditions.Require(fillQty > 0, "fill qty must be positive");
        _totalQty -= fillQty;
    }
}
