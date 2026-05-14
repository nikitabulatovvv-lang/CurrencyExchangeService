using CurrencyExchangeService.Domain.Base;
using CurrencyExchangeService.Domain.Enums;
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

    protected Seller(Guid id, Name name) : base(id)
    {
        Name = name ?? throw new ArgumentNullValueException(nameof(name));
    }

    /// <summary>
    /// Use case: «Создание заявки на продажу».
    /// </summary>
    /// <param name="createdAtUtc">Момент создания заявки (UTC), задаётся снаружи.</param>
    public Order CreateSellOrder(
        Currency baseCurrency,
        Currency quoteCurrency,
        Amount amount,
        Rate rate,
        DateTime createdAtUtc
    )
    {
        if (baseCurrency is null) throw new ArgumentNullValueException(nameof(baseCurrency));
        if (quoteCurrency is null) throw new ArgumentNullValueException(nameof(quoteCurrency));
        if (amount is null) throw new ArgumentNullValueException(nameof(amount));
        if (rate is null) throw new ArgumentNullValueException(nameof(rate));

        var order = new Order(this, baseCurrency, quoteCurrency, amount, rate, createdAtUtc);
        _orders.Add(order);
        return order;
    }

    /// <summary>
    /// Use case: «Редактирование заявки на продажу».
    /// </summary>
    public bool EditSellOrder(Seller actor, Order order, Amount newAmount, Rate newRate)
    {
        if (actor is null) throw new ArgumentNullValueException(nameof(actor));
        if (order is null) throw new ArgumentNullValueException(nameof(order));

        if (!ReferenceEquals(actor, this))
            throw new OrderSellerOwnershipException(actor, this, order, "Редактирование заявки на продажу");

        if (!_orders.Contains(order))
            throw new InvalidOperationException(
                $"Заявка id = {order.Id} не найдена среди заявок продавца «{Name.Value}» (id = {Id})."
            );

        if (!order.IsActive)
            throw new InvalidOperationException(
                $"Заявку id = {order.Id} нельзя редактировать: статус «{order.Status}» (допустимо только «Active»)."
            );

        if (newAmount is null) throw new ArgumentNullValueException(nameof(newAmount));
        if (newRate is null) throw new ArgumentNullValueException(nameof(newRate));

        return order.EditSell(actor, newAmount, newRate);
    }

    /// <summary>
    /// Use case: «Подтверждение сделки» (владелец подтверждает свою заявку на продажу).
    /// </summary>
    public bool ConfirmDeal(Seller actor, Order order)
    {
        if (actor is null) throw new ArgumentNullValueException(nameof(actor));
        if (order is null) throw new ArgumentNullValueException(nameof(order));

        if (!ReferenceEquals(actor, this))
            throw new OrderSellerOwnershipException(actor, this, order, "Подтверждение сделки");

        if (!_orders.Contains(order))
            throw new InvalidOperationException(
                $"Заявка id = {order.Id} не найдена среди заявок продавца «{Name.Value}» (id = {Id})."
            );

        return order.Confirm(actor);
    }

    /// <summary>
    /// Отмена собственной заявки на продажу.
    /// </summary>
    public bool CancelSellOrder(Seller actor, Order order)
    {
        if (actor is null) throw new ArgumentNullValueException(nameof(actor));
        if (order is null) throw new ArgumentNullValueException(nameof(order));

        if (!ReferenceEquals(actor, this))
            throw new OrderSellerOwnershipException(actor, this, order, "Отмена заявки на продажу");

        if (!_orders.Contains(order))
            throw new InvalidOperationException(
                $"Заявка id = {order.Id} не найдена среди заявок продавца «{Name.Value}» (id = {Id})."
            );

        return order.CancelSell(actor);
    }

    /// <summary>
    /// Use case: «Просмотр активных заявок».
    /// </summary>
    public IReadOnlyCollection<Order> GetActiveOrders()
        => _orders.Where(o => o.IsActive).ToList().AsReadOnly();

    /// <summary>
    /// Use case: «Согласие на сделку по чужой активной заявке на покупку».
    /// Продавец принимает заявку типа <see cref="OrderType.Buy"/> (контрагент).
    /// </summary>
    public bool AcceptDeal(Seller actor, Order order)
    {
        if (actor is null) throw new ArgumentNullValueException(nameof(actor));
        if (order is null) throw new ArgumentNullValueException(nameof(order));

        if (!ReferenceEquals(actor, this))
            throw new OrderSellerOwnershipException(actor, this, order, "Принятие заявки на покупку");

        return order.AcceptByCounterparty(actor);
    }
}
