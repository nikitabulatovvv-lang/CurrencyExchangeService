using CurrencyExchangeService.ValueObjects.Base;
using CurrencyExchangeService.ValueObjects.Exceptions;

namespace CurrencyExchangeService.ValueObjects.Validators;

public class CurrencyCodeValidator : IValidator<string>
{
    public static int MAX_LENGTH => 10;
    public static int MIN_LENGTH => 3;

    public void Validate(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullOrWhiteSpaceException(nameof(value));

        if (value.Length > MAX_LENGTH)
            throw new ArgumentLongValueException(nameof(value), value, MAX_LENGTH);

        if (value.Length < MIN_LENGTH)
            throw new ArgumentShortValueException(nameof(value), value, MIN_LENGTH);
    }
}

