namespace CurrencyExchangeService.ValueObjects.Exceptions;

public class ArgumentLongValueException(string paramName, string value, int maxLength)
    : ArgumentOutOfRangeException(
        paramName,
        value,
        $"Argument \"{paramName}\" value length must be <= {maxLength}."
    );

