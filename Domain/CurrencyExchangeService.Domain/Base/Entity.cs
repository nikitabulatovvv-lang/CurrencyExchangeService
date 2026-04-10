
namespace CurrencyExchangeService.Domain.Base;

/// <summary>
/// Base class for all domain entities.
/// </summary>
/// <typeparam name="TId">Entity identifier type.</typeparam>
public abstract class Entity<TId>(TId id)
    where TId : struct, IEquatable<TId>
{
    public TId Id { get; } = id;

    /// <summary>
    /// Protected parameterless ctor for EF Core.
    /// </summary>
    protected Entity() : this(default!)
    {
    }
}
