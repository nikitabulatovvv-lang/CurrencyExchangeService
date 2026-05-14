using CurrencyExchangeService.Domain.Entities;

namespace CurrencyExchangeService.Domain.Exceptions;

/// <summary>
/// Действие над заявкой выполняет не тот покупатель, от имени которого оно должно выполняться.
/// </summary>
public sealed class OrderBuyerOwnershipException(Buyer actor, Buyer expectedOwner, Order order, string actionDescription)
    : InvalidOperationException(
        $"Покупатель «{actor.Name.Value}» не может выполнить действие «{actionDescription}» для заявки id = {order.Id}. " +
        $"Ожидается покупатель «{expectedOwner.Name.Value}» (id = {expectedOwner.Id}).")
{
    public Buyer Actor => actor;
    public Buyer ExpectedOwner => expectedOwner;
    public Order Order => order;
    public string ActionDescription => actionDescription;
}

/// <summary>
/// Действие над заявкой выполняет не тот продавец, от имени которого оно должно выполняться.
/// </summary>
public sealed class OrderSellerOwnershipException(Seller actor, Seller expectedOwner, Order order, string actionDescription)
    : InvalidOperationException(
        $"Продавец «{actor.Name.Value}» не может выполнить действие «{actionDescription}» для заявки id = {order.Id}. " +
        $"Ожидается продавец «{expectedOwner.Name.Value}» (id = {expectedOwner.Id}).")
{
    public Seller Actor => actor;
    public Seller ExpectedOwner => expectedOwner;
    public Order Order => order;
    public string ActionDescription => actionDescription;
}
