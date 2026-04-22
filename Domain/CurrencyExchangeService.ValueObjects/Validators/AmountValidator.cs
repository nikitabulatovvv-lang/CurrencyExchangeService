using CurrencyExchangeService.ValueObjects.Base;

namespace CurrencyExchangeService.ValueObjects.Validators;

public class AmountValidator : IValidator<decimal>
{
    // decimal(18,2): 16 цифр до точки и 2 после точки.
    public static decimal MAX_VALUE => 9999999999999999.99m;
    public static int MAX_SCALE => 2;

    public void Validate(decimal value)
    {
        if (value <= 0m)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Amount must be greater than zero.");

        if (value > MAX_VALUE)
            throw new ArgumentOutOfRangeException(nameof(value), value, $"Amount must be <= {MAX_VALUE}.");

        if (GetScale(value) > MAX_SCALE)
            throw new ArgumentOutOfRangeException(nameof(value), value, $"Amount scale must be <= {MAX_SCALE}.");
    }

    private static int GetScale(decimal value)
        => (decimal.GetBits(value)[3] >> 16) & 0x7F;
}

