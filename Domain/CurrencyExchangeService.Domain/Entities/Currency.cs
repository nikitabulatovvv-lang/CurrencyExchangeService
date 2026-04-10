using CurrencyExchangeService.Domain.Base;
using CurrencyExchangeService.Domain.Exceptions;
using CurrencyExchangeService.ValueObjects;

namespace CurrencyExchangeService.Domain.Entities;

public class Currency : Entity<Guid>
{
    private readonly ICollection<Order> _baseOrders = [];
    private readonly ICollection<Order> _quoteOrders = [];

    public CurrencyCode Code { get; private set; } = default!;

    public IReadOnlyCollection<Order> BaseOrders => _baseOrders.ToList().AsReadOnly();
    public IReadOnlyCollection<Order> QuoteOrders => _quoteOrders.ToList().AsReadOnly();

    protected Currency()
    {
    }

    public Currency(CurrencyCode code)
        : this(Guid.NewGuid(), code)
    {
    }

    public Currency(Guid id, CurrencyCode code) : base(id)
    {
        Code = code ?? throw new ArgumentNullValueException(nameof(code));
    }

    internal bool ChangeCode(CurrencyCode newCode)
    {
        if (newCode == null) throw new ArgumentNullValueException(nameof(newCode));
        if (Code == newCode) return false;
        Code = newCode;
        return true;
    }
}

