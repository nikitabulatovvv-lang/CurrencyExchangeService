using CurrencyExchangeService.Domain.Base;
using CurrencyExchangeService.Domain.Exceptions;
using CurrencyExchangeService.ValueObjects;

namespace CurrencyExchangeService.Domain.Entities;

/// <summary>
/// Покупатель (buyer).
/// 
/// Хранит свои заявки (Orders) и выполняет действия из use-case диаграммы
/// для покупателя: создание заявки на покупку, отмена заявки.
/// </summary>
public class Buyer : Entity<Guid>
{
    private readonly ICollection<Order> _orders = [];

    public Name Name { get; private set; } = default!;

    public IReadOnlyCollection<Order> Orders => _orders.ToList().AsReadOnly();

    protected Buyer()
    {
    }

    public Buyer(Name name)
        : this(Guid.NewGuid(), name)
    {
    }

    public Buyer(Guid id, Name name) : base(id)
    {
        Name = name ?? throw new ArgumentNullValueException(nameof(name));
    }

    /// <summary>
    /// Use case: "Создание заявки на покупку".
    /// Создаёт заявку и добавляет её в коллекцию покупателя.
    /// </summary>
    public Order CreateBuyOrder(
        Currency baseCurrency,
        Currency quoteCurrency,
        decimal amount,
        decimal rate
    )
    {
        var order = Order.CreateBuyOrder(this, baseCurrency, quoteCurrency, amount, rate);
        _orders.Add(order);
        return order;
    }

    /// <summary>
    /// Use case: "Отмена заявки на покупку".
    /// Возвращает true, если статус изменился.
    /// </summary>
    public bool CancelBuyOrder(Order order)
    {
        if (order is null) throw new ArgumentNullException(nameof(order));
        if (order.Buyer != this)
            throw new OrderOwnershipException(this.Id, "cancel_buy_order", order.Id, OrderOwnerType.Buyer, this.Id);

        var result = order.Cancel(this.Id);
        return result;
    }
}

