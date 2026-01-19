namespace MatchingEngine.Domain;

public sealed record Trade(
    long BuyOrderId,
    long SellOrderId,
    long PriceTicks,
    int Qty,
    long AcceptTs,
    long FillTs);
