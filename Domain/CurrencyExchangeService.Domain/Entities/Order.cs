using CurrencyExchangeService.Domain.Base;
using CurrencyExchangeService.Domain.Enums;
using CurrencyExchangeService.Domain.Exceptions;
using CurrencyExchangeService.ValueObjects;

namespace CurrencyExchangeService.Domain.Entities;

/// <summary>
/// Заявка на обмен валют.
///
/// Важно:
/// - для заявки на покупку (<see cref="OrderType.Buy"/>) инициатор — <see cref="Buyer"/> (поле <see cref="Buyer"/>), <see cref="Seller"/> отсутствует до сделки;
/// - для заявки на продажу (<see cref="OrderType.Sell"/>) инициатор — <see cref="Seller"/> (поле <see cref="Seller"/>), <see cref="Buyer"/> отсутствует до сделки;
/// - управляет статусом и пишет историю (<see cref="OrderStatusHistory"/>);
/// - методы изменения статуса получают на вход сущность, от имени которой выполняется действие, и проверяют права.
/// </summary>
public class Order : Entity<Guid>
{
    private readonly ICollection<OrderStatusHistory> _statusHistory = [];

    public OrderType Type { get; private set; }

    /// <summary>Инициатор заявки на покупку (для типа Buy).</summary>
    public Buyer? Buyer { get; private set; }

    /// <summary>Инициатор заявки на продажу (для типа Sell).</summary>
    public Seller? Seller { get; private set; }

    public Currency BaseCurrency { get; private set; } = default!;
    public Currency QuoteCurrency { get; private set; } = default!;

    public Amount Amount { get; private set; } = default!;
    public Rate Rate { get; private set; } = default!;

    public OrderStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory.ToList().AsReadOnly();

    /// <summary>
    /// Активна ли заявка (используется для «просмотра активных заявок»).
    /// </summary>
    public bool IsActive => Status == OrderStatus.Active;

    protected Order()
    {
    }

    protected Order(
        Guid id,
        OrderType type,
        Buyer? buyer,
        Seller? seller,
        Currency baseCurrency,
        Currency quoteCurrency,
        Amount amount,
        Rate rate,
        OrderStatus status,
        DateTime createdAt
    ) : base(id)
    {
        if (baseCurrency is null) throw new ArgumentNullValueException(nameof(baseCurrency));
        if (quoteCurrency is null) throw new ArgumentNullValueException(nameof(quoteCurrency));
        if (amount is null) throw new ArgumentNullValueException(nameof(amount));
        if (rate is null) throw new ArgumentNullValueException(nameof(rate));

        Type = type;
        Buyer = buyer;
        Seller = seller;
        BaseCurrency = baseCurrency;
        QuoteCurrency = quoteCurrency;
        Amount = amount;
        Rate = rate;
        Status = status;
        CreatedAt = createdAt.Kind == DateTimeKind.Utc ? createdAt : DateTime.SpecifyKind(createdAt, DateTimeKind.Utc);
    }

    internal Order(
        Buyer buyer,
        Currency baseCurrency,
        Currency quoteCurrency,
        Amount amount,
        Rate rate,
        DateTime createdAtUtc
    )
        : this(
            Guid.NewGuid(),
            OrderType.Buy,
            buyer,
            seller: null,
            baseCurrency,
            quoteCurrency,
            amount,
            rate,
            OrderStatus.Active,
            createdAtUtc
        )
    {
        if (buyer is null) throw new ArgumentNullValueException(nameof(buyer));
        ValidateOrderData(baseCurrency, quoteCurrency, amount, rate);
    }

    internal Order(
        Seller seller,
        Currency baseCurrency,
        Currency quoteCurrency,
        Amount amount,
        Rate rate,
        DateTime createdAtUtc
    )
        : this(
            Guid.NewGuid(),
            OrderType.Sell,
            buyer: null,
            seller,
            baseCurrency,
            quoteCurrency,
            amount,
            rate,
            OrderStatus.Active,
            createdAtUtc
        )
    {
        if (seller is null) throw new ArgumentNullValueException(nameof(seller));
        ValidateOrderData(baseCurrency, quoteCurrency, amount, rate);
    }

    /// <summary>
    /// Проверка данных заявки (валюты, сумма, курс). Не static: логика относится к сущности заявки.
    /// </summary>
    private void ValidateOrderData(
        Currency baseCurrency,
        Currency quoteCurrency,
        Amount amount,
        Rate rate
    )
    {
        if (baseCurrency is null) throw new ArgumentNullValueException(nameof(baseCurrency));
        if (quoteCurrency is null) throw new ArgumentNullValueException(nameof(quoteCurrency));
        if (amount is null) throw new ArgumentNullValueException(nameof(amount));
        if (rate is null) throw new ArgumentNullValueException(nameof(rate));

        if (baseCurrency.Code == quoteCurrency.Code)
            throw new OrderDataValidationException(
                nameof(baseCurrency),
                "BaseCurrency and QuoteCurrency must be different."
            );
    }

    /// <summary>
    /// Отмена заявки на покупку инициатором-покупателем.
    /// </summary>
    internal bool CancelBuy(Buyer actor)
    {
        if (actor is null) throw new ArgumentNullValueException(nameof(actor));

        if (Type != OrderType.Buy)
            throw new OrderInvalidOrderTypeBuyerException(actor, this, OrderType.Buy, "Отмена заявки на покупку");

        if (Buyer is null)
            throw new InvalidOperationException($"Order '{Id}' is BUY but has no buyer.");

        if (actor.Id != Buyer.Id)
            throw new OrderBuyerOwnershipException(actor, Buyer, this, "Отмена заявки на покупку");

        if (Status != OrderStatus.Active)
            throw new OrderInvalidStatusTransitionBuyerException(
                actor,
                this,
                "Отмена заявки на покупку",
                Status,
                OrderStatus.Cancelled
            );

        return ApplyStatusChange(OrderStatus.Cancelled, DateTime.UtcNow);
    }

    /// <summary>
    /// Отмена заявки на продажу инициатором-продавцом.
    /// </summary>
    internal bool CancelSell(Seller actor)
    {
        if (actor is null) throw new ArgumentNullValueException(nameof(actor));

        if (Type != OrderType.Sell)
            throw new OrderInvalidOrderTypeSellerException(actor, this, OrderType.Sell, "Отмена заявки на продажу");

        if (Seller is null)
            throw new InvalidOperationException($"Order '{Id}' is SELL but has no seller.");

        if (actor.Id != Seller.Id)
            throw new OrderSellerOwnershipException(actor, Seller, this, "Отмена заявки на продажу");

        if (Status != OrderStatus.Active)
            throw new OrderInvalidStatusTransitionSellerException(
                actor,
                this,
                "Отмена заявки на продажу",
                Status,
                OrderStatus.Cancelled
            );

        return ApplyStatusChange(OrderStatus.Cancelled, DateTime.UtcNow);
    }

    /// <summary>
    /// Редактирование заявки на продажу владельцем-продавцом.
    /// </summary>
    internal bool EditSell(Seller actor, Amount newAmount, Rate newRate)
    {
        if (actor is null) throw new ArgumentNullValueException(nameof(actor));

        if (Type != OrderType.Sell)
            throw new OrderInvalidOrderTypeSellerException(actor, this, OrderType.Sell, "Редактирование заявки на продажу");

        if (Seller is null)
            throw new InvalidOperationException($"Order '{Id}' is SELL but has no seller.");

        if (actor.Id != Seller.Id)
            throw new OrderSellerOwnershipException(actor, Seller, this, "Редактирование заявки на продажу");

        if (Status != OrderStatus.Active)
            throw new InvalidOperationException($"Заявку id = {Id} нельзя редактировать в статусе «{Status}» (допустимо только «{OrderStatus.Active}»).");

        if (newAmount is null) throw new ArgumentNullValueException(nameof(newAmount));
        if (newRate is null) throw new ArgumentNullValueException(nameof(newRate));

        var isChanged = Amount != newAmount || Rate != newRate;
        if (!isChanged) return false;

        Amount = newAmount;
        Rate = newRate;
        return true;
    }

    /// <summary>
    /// Редактирование заявки на покупку владельцем-покупателем.
    /// </summary>
    internal bool EditBuy(Buyer actor, Amount newAmount, Rate newRate)
    {
        if (actor is null) throw new ArgumentNullValueException(nameof(actor));

        if (Type != OrderType.Buy)
            throw new OrderInvalidOrderTypeBuyerException(actor, this, OrderType.Buy, "Редактирование заявки на покупку");

        if (Buyer is null || actor.Id != Buyer.Id)
            throw new OrderBuyerOwnershipException(actor, Buyer ?? actor, this, "Редактирование заявки на покупку");

        if (Status != OrderStatus.Active)
            throw new InvalidOperationException($"Заявку id = {Id} нельзя редактировать в статусе «{Status}» (допустимо только «{OrderStatus.Active}»).");

        if (newAmount is null) throw new ArgumentNullValueException(nameof(newAmount));
        if (newRate is null) throw new ArgumentNullValueException(nameof(newRate));

        var isChanged = Amount != newAmount || Rate != newRate;
        if (!isChanged) return false;

        Amount = newAmount;
        Rate = newRate;
        return true;
    }

    /// <summary>
    /// Подтверждение сделки владельцем заявки на продажу (перевод в Completed).
    /// </summary>
    internal bool Confirm(Seller actor)
    {
        if (actor is null) throw new ArgumentNullValueException(nameof(actor));

        if (Type != OrderType.Sell)
            throw new OrderInvalidOrderTypeSellerException(actor, this, OrderType.Sell, "Подтверждение сделки");

        if (Seller is null)
            throw new InvalidOperationException($"Order '{Id}' is SELL but has no seller.");

        if (actor.Id != Seller.Id)
            throw new OrderSellerOwnershipException(actor, Seller, this, "Подтверждение сделки");

        if (Status != OrderStatus.Active)
            throw new OrderInvalidStatusTransitionSellerException(
                actor,
                this,
                "Подтверждение сделки",
                Status,
                OrderStatus.Completed
            );

        return ApplyStatusChange(OrderStatus.Completed, DateTime.UtcNow);
    }

    /// <summary>
    /// Покупатель принимает чужую активную заявку на продажу (контрагент по SELL).
    /// </summary>
    internal bool AcceptByCounterparty(Buyer actor)
    {
        if (actor is null) throw new ArgumentNullValueException(nameof(actor));

        if (Type != OrderType.Sell)
            throw new OrderInvalidOrderTypeBuyerException(actor, this, OrderType.Sell, "Принятие заявки на продажу");

        if (Seller is null)
            throw new InvalidOperationException($"Order '{Id}' has no seller.");

        if (Status != OrderStatus.Active)
            throw new OrderInvalidStatusTransitionBuyerException(
                actor,
                this,
                "Принятие заявки на продажу",
                Status,
                OrderStatus.Completed
            );

        return ApplyStatusChange(OrderStatus.Completed, DateTime.UtcNow);
    }

    /// <summary>
    /// Продавец принимает чужую активную заявку на покупку (контрагент по BUY).
    /// </summary>
    internal bool AcceptByCounterparty(Seller actor)
    {
        if (actor is null) throw new ArgumentNullValueException(nameof(actor));

        if (Type != OrderType.Buy)
            throw new OrderInvalidOrderTypeSellerException(actor, this, OrderType.Buy, "Принятие заявки на покупку");

        if (Buyer is null)
            throw new InvalidOperationException($"Order '{Id}' has no buyer.");

        if (Status != OrderStatus.Active)
            throw new OrderInvalidStatusTransitionSellerException(
                actor,
                this,
                "Принятие заявки на покупку",
                Status,
                OrderStatus.Completed
            );

        return ApplyStatusChange(OrderStatus.Completed, DateTime.UtcNow);
    }

    private bool ApplyStatusChange(OrderStatus newStatus, DateTime changedAtUtc)
    {
        if (Status == newStatus) return false;

        var oldStatus = Status;
        var history = OrderStatusHistory.Create(this, oldStatus, newStatus, changedAtUtc);
        _statusHistory.Add(history);
        Status = newStatus;
        return true;
    }
}
