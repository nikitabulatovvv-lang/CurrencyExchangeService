using Microsoft.EntityFrameworkCore;
using CurrencyExchangeService.Domain.Base;
using CurrencyExchangeService.Domain.Repositories.Abstractions.Base;
using CurrencyExchangeService.Infrastructure.EntityFramework;

namespace CurrencyExchangeService.Infrastructure.EntityFramework.RepositoriesEF;

public class EfRepository<TEntity, TId>(ApplicationDbContext context)
    : IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : struct, IEquatable<TId>
{
    public async Task<IReadOnlyCollection<TEntity>> GetAllAsync(CancellationToken cancellationToken, bool asNoTracking = false)
    {
        var items = await (asNoTracking ? context.Set<TEntity>().AsNoTracking() : context.Set<TEntity>())
            .ToListAsync(cancellationToken);

        return items.AsReadOnly();
    }

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken)
        => await context.Set<TEntity>().FindAsync(id, cancellationToken);

    public async Task<TEntity?> AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        if (entity is null) return null;

        await context.Set<TEntity>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public async Task<bool> UpdateAsync(TEntity entity, CancellationToken cancellationToken)
    {
        if (entity is null) return false;

        context.Set<TEntity>().Update(entity);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(TEntity entity, CancellationToken cancellationToken)
    {
        if (entity is null) return false;

        context.Set<TEntity>().Remove(entity);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<bool> DeleteAsync(TId id, CancellationToken cancellationToken)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        return entity is not null && await DeleteAsync(entity, cancellationToken);
    }
}

