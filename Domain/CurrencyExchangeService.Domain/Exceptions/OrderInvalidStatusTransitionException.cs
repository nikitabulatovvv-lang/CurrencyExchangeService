using CurrencyExchangeService.Domain.Enums;

namespace CurrencyExchangeService.Domain.Exceptions;

public class OrderInvalidStatusTransitionException(
    Guid actorId,
    string action,
    Guid orderId,
    OrderStatus oldStatus,
    OrderStatus newStatus
) : InvalidOperationException(
    $"Action '{action}' can't be performed by actor '{actorId}' for order '{orderId}'. Status transition '{oldStatus}' -> '{newStatus}' is not allowed.")
{
    public Guid ActorId { get; } = actorId;
    public string Action { get; } = action;
    public Guid OrderId { get; } = orderId;
    public OrderStatus OldStatus { get; } = oldStatus;
    public OrderStatus NewStatus { get; } = newStatus;
}

