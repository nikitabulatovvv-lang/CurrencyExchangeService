using CurrencyExchangeService.ValueObjects.Base;
using CurrencyExchangeService.ValueObjects.Validators;

namespace CurrencyExchangeService.ValueObjects;

/// <summary>
/// Обозначает курс обмена.
/// </summary>
public sealed class Rate(decimal value)
    : ValueObject<decimal>(new RateValidator(), value);

