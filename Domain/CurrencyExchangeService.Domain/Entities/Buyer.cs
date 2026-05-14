using CurrencyExchangeService.Domain.Base;
using CurrencyExchangeService.Domain.Exceptions;
using CurrencyExchangeService.Domain.Enums;
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

    protected Buyer(Guid id, Name name) : base(id)
    {
        Name = name ?? throw new ArgumentNullValueException(nameof(name));
    }

    /// <summary>
    /// Use case: «Создание заявки на покупку».
    /// Создаёт заявку и добавляет её в коллекцию покупателя.
    /// </summary>
    /// <param name="createdAtUtc">Момент создания заявки (UTC), задаётся снаружи.</param>
    public Order CreateBuyOrder(
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
    /// Use case: «Отмена заявки на покупку».
    /// <paramref name="actor"/> — покупатель, который выполняет отмену (должен совпадать с <c>this</c> и быть владельцем заявки).
    /// </summary>
    public bool CancelBuyOrder(Buyer actor, Order order)
    {
        if (actor is null) throw new ArgumentNullValueException(nameof(actor));
        if (order is null) throw new ArgumentNullValueException(nameof(order));

        if (!ReferenceEquals(actor, this))
            throw new OrderBuyerOwnershipException(actor, this, order, "Отмена заявки на покупку");

        if (!_orders.Contains(order))
            throw new InvalidOperationException(
                $"Заявка id = {order.Id} не найдена среди заявок покупателя «{Name.Value}» (id = {Id})."
            );

        return order.CancelBuy(actor);
    }

    /// <summary>
    /// Use case: «Редактирование заявки на покупку».
    /// Редактировать можно только активную заявку из своего списка.
    /// </summary>
    public bool EditBuyOrder(Buyer actor, Order order, Amount newAmount, Rate newRate)
    {
        if (actor is null) throw new ArgumentNullValueException(nameof(actor));
        if (order is null) throw new ArgumentNullValueException(nameof(order));

        if (!ReferenceEquals(actor, this))
            throw new OrderBuyerOwnershipException(actor, this, order, "Редактирование заявки на покупку");

        if (!_orders.Contains(order))
            throw new InvalidOperationException(
                $"Заявка id = {order.Id} не найдена среди заявок покупателя «{Name.Value}» (id = {Id})."
            );

        if (!order.IsActive)
            throw new InvalidOperationException(
                $"Заявку id = {order.Id} нельзя редактировать: статус «{order.Status}» (допустимо только «Active»)."
            );

        if (newAmount is null) throw new ArgumentNullValueException(nameof(newAmount));
        if (newRate is null) throw new ArgumentNullValueException(nameof(newRate));

        return order.EditBuy(actor, newAmount, newRate);
    }

    /// <summary>
    /// Use case: «Согласие на сделку по чужой активной заявке на продажу».
    /// Покупатель принимает заявку типа <see cref="OrderType.Sell"/> (контрагент).
    /// Заявки на покупку (<see cref="OrderType.Buy"/>) принимает продавец — см. <see cref="Seller.AcceptDeal"/>.
    /// </summary>
    public bool AcceptDeal(Buyer actor, Order order)
    {
        if (actor is null) throw new ArgumentNullValueException(nameof(actor));
        if (order is null) throw new ArgumentNullValueException(nameof(order));

        if (!ReferenceEquals(actor, this))
            throw new OrderBuyerOwnershipException(actor, this, order, "Принятие заявки на продажу");

        return order.AcceptByCounterparty(actor);
    }
}
