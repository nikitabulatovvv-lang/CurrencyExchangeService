namespace CurrencyExchangeService.Domain.Exceptions;

public class OrderDataValidationException(string paramName, object? value)
    : ArgumentException($"Invalid order data: '{paramName}' value '{value}' is not allowed.")
{
    public string DataParamName { get; } = paramName;
    public object? Value { get; } = value;
}

