using CurrencyExchangeService.Domain.Enums;

namespace CurrencyExchangeService.Domain.Exceptions;

public class OrderInvalidOrderTypeException(
    Guid actorId,
    string action,
    OrderType expectedType,
    OrderType actualType
) : InvalidOperationException(
    $"Action '{action}' can't be performed by actor '{actorId}'. Expected order type '{expectedType}', actual '{actualType}'.")
{
    public Guid ActorId { get; } = actorId;
    public string Action { get; } = action;
    public OrderType ExpectedType { get; } = expectedType;
    public OrderType ActualType { get; } = actualType;
}

