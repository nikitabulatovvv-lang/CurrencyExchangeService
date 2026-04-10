using CurrencyExchangeService.Domain.Base;
using CurrencyExchangeService.Domain.Exceptions;
using CurrencyExchangeService.ValueObjects;

namespace CurrencyExchangeService.Domain.Entities;

/// <summary>
/// Продавец (seller).
/// 
/// Хранит свои заявки (Orders) и выполняет действия из use-case диаграммы
/// для продавца: создание/редактирование заявки на продажу, просмотр активных,
/// подтверждение сделки.
/// </summary>
public class Seller : Entity<Guid>
{
    private readonly ICollection<Order> _orders = [];

    public Name Name { get; private set; } = default!;

    public IReadOnlyCollection<Order> Orders => _orders.ToList().AsReadOnly();

    protected Seller()
    {
    }

    public Seller(Name name)
        : this(Guid.NewGuid(), name)
    {
    }

    public Seller(Guid id, Name name) : base(id)
    {
        Name = name ?? throw new ArgumentNullValueException(nameof(name));
    }

    /// <summary>
    /// Use case: "Создание заявки на продажу".
    /// Создаёт заявку и добавляет её в коллекцию продавца.
    /// </summary>
    public Order CreateSellOrder(
        Currency baseCurrency,
        Currency quoteCurrency,
        decimal amount,
        decimal rate
    )
    {
        var order = Order.CreateSellOrder(this, baseCurrency, quoteCurrency, amount, rate);
        _orders.Add(order);
        return order;
    }

    /// <summary>
    /// Use case: "Редактирование заявки на продажу".
    /// Возвращает true, если данные реально изменились.
    /// </summary>
    public bool EditSellOrder(Order order, decimal newAmount, decimal newRate)
    {
        if (order is null) throw new ArgumentNullException(nameof(order));
        if (order.Seller != this)
            throw new OrderOwnershipException(this.Id, "edit_sell_order", order.Id, OrderOwnerType.Seller, this.Id);

        return order.EditSell(this.Id, newAmount, newRate);
    }

    /// <summary>
    /// Use case: "Подтверждение сделки".
    /// Переводит заявку в Completed (если это допустимо).
    /// </summary>
    public bool ConfirmDeal(Order order)
    {
        if (order is null) throw new ArgumentNullException(nameof(order));
        return order.Confirm(this.Id);
    }

    /// <summary>
    /// Use case: "Просмотр активных заявок".
    /// Возвращает только те заявки, у которых Status == Active.
    /// </summary>
    public IReadOnlyCollection<Order> GetActiveOrders()
        => _orders.Where(o => o.IsActive).ToList().AsReadOnly();
}

