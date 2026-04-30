using CurrencyExchangeService.ValueObjects.Base;

namespace CurrencyExchangeService.ValueObjects.Validators;

public class RateValidator : IValidator<decimal>
{
    // decimal(18,6): 12 цифр до точки и 6 после точки.
    public static decimal MAX_VALUE => 999999999999.999999m;
    public static int MAX_SCALE => 6;

    public void Validate(decimal value)
    {
        if (value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Rate must be greater than zero.");

        if (value > MAX_VALUE)
            throw new ArgumentOutOfRangeException(nameof(value), value, $"Rate must be <= {MAX_VALUE}.");

        if (GetScale(value) > MAX_SCALE)
            throw new ArgumentOutOfRangeException(nameof(value), value, $"Rate scale must be <= {MAX_SCALE}.");
    }

    private static int GetScale(decimal value)
        => (decimal.GetBits(value)[3] >> 16) & 0x7F;
}

