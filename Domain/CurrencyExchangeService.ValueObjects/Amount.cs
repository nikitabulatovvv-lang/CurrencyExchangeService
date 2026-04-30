using CurrencyExchangeService.ValueObjects.Base;
using CurrencyExchangeService.ValueObjects.Validators;

namespace CurrencyExchangeService.ValueObjects;

/// <summary>
/// Обозначает сумму заявки.
/// </summary>
public sealed class Amount(decimal value)
    : ValueObject<decimal>(new AmountValidator(), value);

