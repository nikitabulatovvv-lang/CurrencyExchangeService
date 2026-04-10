namespace CurrencyExchangeService.Domain.Exceptions;

public class OrderOwnershipException(
    Guid actorId,
    string action,
    Guid orderId,
    OrderOwnerType expectedOwnerType,
    Guid? expectedOwnerId
) : InvalidOperationException(
    $"Action '{action}' can't be performed on order '{orderId}'. Actor '{actorId}' is not the expected owner '{expectedOwnerType}' (expected owner id = '{expectedOwnerId}').")
{
    public Guid ActorId { get; } = actorId;
    public string Action { get; } = action;
    public Guid OrderId { get; } = orderId;
    public OrderOwnerType ExpectedOwnerType { get; } = expectedOwnerType;
    public Guid? ExpectedOwnerId { get; } = expectedOwnerId;
}

