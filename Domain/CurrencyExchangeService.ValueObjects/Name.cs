using CurrencyExchangeService.ValueObjects.Base;
using CurrencyExchangeService.ValueObjects.Validators;

namespace CurrencyExchangeService.ValueObjects;

/// <summary>
/// Обозначает имя покупателя/продавца.
/// </summary>
public sealed class Name(string name)
    : ValueObject<string>(new NameValidator(), (name ?? string.Empty).Trim());

