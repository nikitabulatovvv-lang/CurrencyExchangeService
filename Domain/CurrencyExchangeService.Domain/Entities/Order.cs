using CurrencyExchangeService.Domain.Base;
using CurrencyExchangeService.Domain.Enums;
using CurrencyExchangeService.Domain.Exceptions;

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

    public decimal Amount { get; private set; }
    public decimal Rate { get; private set; }

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
        decimal amount,
        decimal rate,
        OrderStatus status,
        DateTime createdAt
    ) : base(id)
    {
        Type = type;
        Buyer = buyer;
        Seller = seller;

        BaseCurrency = baseCurrency ?? throw new ArgumentNullValueException(nameof(baseCurrency));
        QuoteCurrency = quoteCurrency ?? throw new ArgumentNullValueException(nameof(quoteCurrency));
        if (baseCurrency.Id == quoteCurrency.Id)
            throw new OrderDataValidationException(
                nameof(baseCurrency),
                "BaseCurrency and QuoteCurrency must be different."
            );

        if (amount <= 0) throw new OrderDataValidationException(nameof(amount), amount);
        if (rate <= 0) throw new OrderDataValidationException(nameof(rate), rate);

        Amount = amount;
        Rate = rate;
        Status = status;
        CreatedAt = createdAt.Kind == DateTimeKind.Utc ? createdAt : DateTime.SpecifyKind(createdAt, DateTimeKind.Utc);
    }

    /// <summary>
    /// Use case: "Создание заявки на покупку".
    /// </summary>
    internal static Order CreateBuyOrder(Buyer buyer, Currency baseCurrency, Currency quoteCurrency, decimal amount, decimal rate)
    {
        if (buyer is null) throw new ArgumentNullException(nameof(buyer));
        return new Order(
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
        );
    }

    /// <summary>
    /// Use case: "Создание заявки на продажу".
    /// </summary>
    internal static Order CreateSellOrder(Seller seller, Currency baseCurrency, Currency quoteCurrency, decimal amount, decimal rate)
    {
        if (seller is null) throw new ArgumentNullException(nameof(seller));
        return new Order(
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
        );
    }

    /// <summary>
    /// Use case: "Отмена заявки на покупку".
    /// </summary>
    internal bool Cancel(Guid actorId)
    {
        if (Type != OrderType.Buy)
            throw new OrderInvalidOrderTypeException(actorId, "cancel_buy_order", OrderType.Buy, Type);

        if (Buyer is null || Buyer.Id != actorId)
            throw new OrderOwnershipException(actorId, "cancel_buy_order", Id, OrderOwnerType.Buyer, Buyer?.Id);

        if (Status != OrderStatus.Active)
            throw new OrderInvalidStatusTransitionException(actorId, "cancel_buy_order", Id, Status, OrderStatus.Cancelled);

        return ApplyStatusChange(OrderStatus.Cancelled);
    }

    /// <summary>
    /// Use case: "Редактирование заявки на продажу".
    /// </summary>
    internal bool EditSell(Guid actorId, decimal newAmount, decimal newRate)
    {
        if (Type != OrderType.Sell)
            throw new OrderInvalidOrderTypeException(actorId, "edit_sell_order", OrderType.Sell, Type);

        if (Seller is null || Seller.Id != actorId)
            throw new OrderOwnershipException(actorId, "edit_sell_order", Id, OrderOwnerType.Seller, Seller?.Id);

        if (Status != OrderStatus.Active)
            throw new OrderInvalidStatusTransitionException(actorId, "edit_sell_order", Id, Status, Status);

        if (newAmount <= 0) throw new OrderDataValidationException(nameof(newAmount), newAmount);
        if (newRate <= 0) throw new OrderDataValidationException(nameof(newRate), newRate);

        var isChanged = Amount != newAmount || Rate != newRate;
        if (!isChanged) return false;

        Amount = newAmount;
        Rate = newRate;
        return true;
    }

    /// <summary>
    /// Use case: "Подтверждение сделки".
    /// </summary>
    internal bool Confirm(Guid actorId)
    {
        var expectedOwner = Type == OrderType.Buy ? OrderOwnerType.Buyer : OrderOwnerType.Seller;

        if (Status != OrderStatus.Active)
            throw new OrderInvalidStatusTransitionException(actorId, "confirm_deal", Id, Status, OrderStatus.Completed);

        if (expectedOwner == OrderOwnerType.Buyer)
        {
            if (Buyer is null || Buyer.Id != actorId)
                throw new OrderOwnershipException(actorId, "confirm_deal", Id, expectedOwner, Buyer?.Id);
        }
        else
        {
            if (Seller is null || Seller.Id != actorId)
                throw new OrderOwnershipException(actorId, "confirm_deal", Id, expectedOwner, Seller?.Id);
        }

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

