using CurrencyExchangeService.Domain.Base;
using CurrencyExchangeService.Domain.Enums;
using CurrencyExchangeService.Domain.Exceptions;
using CurrencyExchangeService.ValueObjects;

namespace CurrencyExchangeService.Domain.Entities;

/// <summary>
/// Заявка на обмен валют.
/// 
/// Важно:
/// - хранит ссылки на Buyer/Seller и Currency
/// - управляет своим статусом и пишет историю смены статусов (OrderStatusHistory)
/// - методы use-case диаграммы (создание/редактирование/отмена/подтверждение)
/// </summary>
public class Order : Entity<Guid>
{
    private readonly ICollection<OrderStatusHistory> _statusHistory = [];

    public OrderType Type { get; private set; }

    public Buyer? Buyer { get; private set; }
    public Seller? Seller { get; private set; }

    public Currency BaseCurrency { get; private set; } = default!;
    public Currency QuoteCurrency { get; private set; } = default!;

    public Amount Amount { get; private set; } = default!;
    public Rate Rate { get; private set; } = default!;

    public OrderStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<OrderStatusHistory> StatusHistory => _statusHistory.ToList().AsReadOnly();

    /// <summary>
    /// Активна ли заявка (используется для "просмотра активных заявок").
    /// </summary>
    public bool IsActive => Status == OrderStatus.Active;

    protected Order()
    {
    }

    private Order(
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

    internal Order(Buyer buyer, Currency baseCurrency, Currency quoteCurrency, Amount amount, Rate rate)
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
            DateTime.UtcNow
        )
    {
        if (buyer is null) throw new ArgumentNullValueException(nameof(buyer));
        ValidateOrderData(baseCurrency, quoteCurrency, amount, rate);
    }

    internal Order(Seller seller, Currency baseCurrency, Currency quoteCurrency, Amount amount, Rate rate)
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
            DateTime.UtcNow
        )
    {
        if (seller is null) throw new ArgumentNullValueException(nameof(seller));
        ValidateOrderData(baseCurrency, quoteCurrency, amount, rate);
    }

    private static void ValidateOrderData(
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
    /// Use case: "Отмена заявки на покупку".
    /// </summary>
    internal bool CancelBuy()
    {
        if (Type != OrderType.Buy)
            throw new InvalidOperationException($"Order '{Id}' is not BUY and cannot be cancelled by buyer flow.");

        if (Status != OrderStatus.Active)
            throw new InvalidOperationException($"Order '{Id}' cannot be cancelled from status '{Status}'.");

        return ApplyStatusChange(OrderStatus.Cancelled);
    }

    /// <summary>
    /// Use case: "Отмена заявки на продажу".
    /// </summary>
    internal bool CancelSell()
    {
        if (Type != OrderType.Sell)
            throw new InvalidOperationException($"Order '{Id}' is not SELL and cannot be cancelled by seller flow.");

        if (Status != OrderStatus.Active)
            throw new InvalidOperationException($"Order '{Id}' cannot be cancelled from status '{Status}'.");

        return ApplyStatusChange(OrderStatus.Cancelled);
    }

    /// <summary>
    /// Use case: "Редактирование заявки на продажу".
    /// </summary>
    internal bool EditSell(Amount newAmount, Rate newRate)
    {
        if (Type != OrderType.Sell)
            throw new InvalidOperationException($"Order '{Id}' is not SELL and cannot be edited by seller flow.");

        if (Status != OrderStatus.Active)
            throw new InvalidOperationException($"Order '{Id}' cannot be edited in status '{Status}'.");

        if (newAmount is null) throw new ArgumentNullValueException(nameof(newAmount));
        if (newRate is null) throw new ArgumentNullValueException(nameof(newRate));

        var isChanged = Amount != newAmount || Rate != newRate;
        if (!isChanged) return false;

        Amount = newAmount;
        Rate = newRate;
        return true;
    }

    /// <summary>
    /// Use case: "Редактирование заявки на покупку".
    /// </summary>
    internal bool EditBuy(Amount newAmount, Rate newRate)
    {
        if (Type != OrderType.Buy)
            throw new InvalidOperationException($"Order '{Id}' is not BUY and cannot be edited by buyer flow.");

        if (Status != OrderStatus.Active)
            throw new InvalidOperationException($"Order '{Id}' cannot be edited in status '{Status}'.");

        if (newAmount is null) throw new ArgumentNullValueException(nameof(newAmount));
        if (newRate is null) throw new ArgumentNullValueException(nameof(newRate));

        var isChanged = Amount != newAmount || Rate != newRate;
        if (!isChanged) return false;

        Amount = newAmount;
        Rate = newRate;
        return true;
    }

    /// <summary>
    /// Use case: "Подтверждение сделки".
    /// </summary>
    internal bool Confirm()
    {
        if (Status != OrderStatus.Active)
            throw new InvalidOperationException($"Order '{Id}' cannot be confirmed from status '{Status}'.");

        return ApplyStatusChange(OrderStatus.Completed);
    }

    /// <summary>
    /// Согласие на сделку второй стороной (контрагентом).
    /// </summary>
    internal bool AcceptByCounterparty()
    {
        if (Status != OrderStatus.Active)
            throw new InvalidOperationException($"Order '{Id}' cannot be accepted from status '{Status}'.");

        return ApplyStatusChange(OrderStatus.Completed);
    }

    private bool ApplyStatusChange(OrderStatus newStatus)
    {
        if (Status == newStatus) return false;

        var oldStatus = Status;
        var history = new OrderStatusHistory(this, oldStatus, newStatus, DateTime.UtcNow);
        _statusHistory.Add(history);
        Status = newStatus;
        return true;
    }
}

