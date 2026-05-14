using CurrencyExchangeService.Domain.Entities;
using CurrencyExchangeService.Domain.Enums;

namespace CurrencyExchangeService.Domain.Exceptions;

/// <summary>
/// Недопустимый переход статуса заявки при действии от имени покупателя.
/// </summary>
public sealed class OrderInvalidStatusTransitionBuyerException(
    Buyer actor,
    Order order,
    string actionDescription,
    OrderStatus currentStatus,
    OrderStatus attemptedStatus
) : InvalidOperationException(
    $"Покупатель «{actor.Name.Value}» не может выполнить «{actionDescription}» для заявки id = {order.Id}: " +
    $"текущий статус «{currentStatus}», запрошенный переход к «{attemptedStatus}» недопустим.")
{
    public Buyer Actor => actor;
    public Order Order => order;
    public string ActionDescription => actionDescription;
    public OrderStatus CurrentStatus => currentStatus;
    public OrderStatus AttemptedStatus => attemptedStatus;
}

/// <summary>
/// Недопустимый переход статуса заявки при действии от имени продавца.
/// </summary>
public sealed class OrderInvalidStatusTransitionSellerException(
    Seller actor,
    Order order,
    string actionDescription,
    OrderStatus currentStatus,
    OrderStatus attemptedStatus
) : InvalidOperationException(
    $"Продавец «{actor.Name.Value}» не может выполнить «{actionDescription}» для заявки id = {order.Id}: " +
    $"текущий статус «{currentStatus}», запрошенный переход к «{attemptedStatus}» недопустим.")
{
    public Seller Actor => actor;
    public Order Order => order;
    public string ActionDescription => actionDescription;
    public OrderStatus CurrentStatus => currentStatus;
    public OrderStatus AttemptedStatus => attemptedStatus;
}
