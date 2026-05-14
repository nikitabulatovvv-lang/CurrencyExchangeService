using CurrencyExchangeService.Domain.Base;
using CurrencyExchangeService.Domain.Enums;

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

    /// <summary>
    /// Создаёт запись истории (используется из <see cref="Order"/>).
    /// </summary>
    internal static OrderStatusHistory Create(
        Order order,
        OrderStatus oldStatus,
        OrderStatus newStatus,
        DateTime changedAtUtc
    )
    {
        if (order is null) throw new ArgumentNullValueException(nameof(order));

        var utc = changedAtUtc.Kind == DateTimeKind.Utc
            ? changedAtUtc
            : DateTime.SpecifyKind(changedAtUtc, DateTimeKind.Utc);

        return new OrderStatusHistory(Guid.NewGuid(), order, oldStatus, newStatus, utc);
    }

    protected OrderStatusHistory(
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
