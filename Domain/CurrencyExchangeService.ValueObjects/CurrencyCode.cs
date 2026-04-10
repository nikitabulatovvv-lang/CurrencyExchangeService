using CurrencyExchangeService.ValueObjects.Base;
using CurrencyExchangeService.ValueObjects.Validators;

namespace CurrencyExchangeService.ValueObjects;

/// <summary>
/// Обозначает код валюты (например, USD, EUR).
/// </summary>
public sealed class CurrencyCode(string code)
    : ValueObject<string>(
        new CurrencyCodeValidator(),
        (code ?? string.Empty).Trim().ToUpperInvariant()
    );

