using CurrencyExchangeService.Domain.Entities;
using CurrencyExchangeService.Domain.Enums;

namespace CurrencyExchangeService.Domain.Exceptions;

/// <summary>
/// Покупатель пытается выполнить действие, допустимое только для заявки другого типа.
/// </summary>
public sealed class OrderInvalidOrderTypeBuyerException(Buyer actor, Order order, OrderType expectedType, string actionDescription)
    : InvalidOperationException(
        $"Покупатель «{actor.Name.Value}» не может выполнить «{actionDescription}» для заявки id = {order.Id}: " +
        $"фактический тип заявки «{order.Type}», ожидался «{expectedType}».")
{
    public Buyer Actor => actor;
    public Order Order => order;
    public OrderType ExpectedType => expectedType;
    public string ActionDescription => actionDescription;
}

/// <summary>
/// Продавец пытается выполнить действие, допустимое только для заявки другого типа.
/// </summary>
public sealed class OrderInvalidOrderTypeSellerException(Seller actor, Order order, OrderType expectedType, string actionDescription)
    : InvalidOperationException(
        $"Продавец «{actor.Name.Value}» не может выполнить «{actionDescription}» для заявки id = {order.Id}: " +
        $"фактический тип заявки «{order.Type}», ожидался «{expectedType}».")
{
    public Seller Actor => actor;
    public Order Order => order;
    public OrderType ExpectedType => expectedType;
    public string ActionDescription => actionDescription;
}
