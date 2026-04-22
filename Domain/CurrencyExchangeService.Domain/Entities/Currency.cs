using CurrencyExchangeService.Domain.Base;
using CurrencyExchangeService.Domain.Exceptions;
using CurrencyExchangeService.ValueObjects;

namespace CurrencyExchangeService.Domain.Entities;

public class Currency : Entity<Guid>
{
    public CurrencyCode Code { get; private set; } = default!;

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
}

