using CurrencyExchangeService.Domain.Base;
using CurrencyExchangeService.Domain.Enums;
using CurrencyExchangeService.Domain.Exceptions;

namespace CurrencyExchangeService.Domain.Entities;

public class OrderStatusHistory : Entity<Guid>
{
    public Order Order { get; private set; } = default!;

    public OrderStatus OldStatus { get; private set; }
    public OrderStatus NewStatus { get; private set; }

    public DateTime ChangedAt { get; private set; }

    protected OrderStatusHistory()
    {
    }

    public OrderStatusHistory(Order order, OrderStatus oldStatus, OrderStatus newStatus, DateTime changedAt)
        : this(Guid.NewGuid(), order, oldStatus, newStatus, changedAt)
    {
    }

    public OrderStatusHistory(
        Guid id,
        Order order,
        OrderStatus oldStatus,
        OrderStatus newStatus,
        DateTime changedAt
    ) : base(id)
    {
        Order = order ?? throw new ArgumentNullValueException(nameof(order));

        OldStatus = oldStatus;
        NewStatus = newStatus;
        ChangedAt = changedAt;
    }
}

