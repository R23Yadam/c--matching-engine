using MatchingEngine.Domain;
using MatchingEngine.Util;

namespace MatchingEngine.Analytics;

public sealed class PnLTracker
{
    public int Position { get; private set; }
    public long AvgCostTicks { get; private set; }
    public long RealizedPnlTicks { get; private set; }
    public long MarkPriceTicks { get; private set; }

    public long UnrealizedPnlTicks => Position == 0 ? 0 : (MarkPriceTicks - AvgCostTicks) * Position;
    public long TotalPnlTicks => RealizedPnlTicks + UnrealizedPnlTicks;

    public void UpdateMark(long markPriceTicks)
    {
        Preconditions.Require(markPriceTicks > 0, "mark price must be positive");
        MarkPriceTicks = markPriceTicks;
    }

    public void ApplyFill(Side side, long priceTicks, int qty)
    {
        Preconditions.Require(qty > 0, "qty must be positive");

        if (side == Side.Buy)
        {
            ApplyBuy(priceTicks, qty);
            return;
        }

        ApplySell(priceTicks, qty);
    }

    private void ApplyBuy(long priceTicks, int qty)
    {
        if (Position >= 0)
        {
            var newPosition = Position + qty;
            AvgCostTicks = Position == 0
                ? priceTicks
                : WeightedAverage(AvgCostTicks, Position, priceTicks, qty);
            Position = newPosition;
            return;
        }

        var closingQty = Math.Min(qty, -Position);
        RealizedPnlTicks += (AvgCostTicks - priceTicks) * closingQty;
        Position += closingQty;
        var remainingQty = qty - closingQty;

        if (Position == 0 && remainingQty > 0)
        {
            AvgCostTicks = priceTicks;
            Position = remainingQty;
        }
    }

    private void ApplySell(long priceTicks, int qty)
    {
        if (Position <= 0)
        {
            var newPosition = Position - qty;
            AvgCostTicks = Position == 0
                ? priceTicks
                : WeightedAverage(AvgCostTicks, -Position, priceTicks, qty);
            Position = newPosition;
            return;
        }

        var closingQty = Math.Min(qty, Position);
        RealizedPnlTicks += (priceTicks - AvgCostTicks) * closingQty;
        Position -= closingQty;
        var remainingQty = qty - closingQty;

        if (Position == 0 && remainingQty > 0)
        {
            AvgCostTicks = priceTicks;
            Position = -remainingQty;
        }
    }

    private static long WeightedAverage(long priceA, int qtyA, long priceB, int qtyB)
    {
        var totalQty = qtyA + qtyB;
        return (priceA * qtyA + priceB * qtyB) / totalQty;
    }
}
