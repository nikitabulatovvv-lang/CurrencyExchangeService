using Microsoft.EntityFrameworkCore;
using CurrencyExchangeService.Domain.Entities;

namespace CurrencyExchangeService.Infrastructure.EntityFramework;

/// <summary>
/// Контекст базы данных (EF Core).
/// 
/// Здесь определяются "таблицы" через DbSet, а также подключаются конфигурации
/// из папки Configurations (маппинг сущностей домена на таблицы PostgreSQL).
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    /// <summary>Таблица buyers</summary>
    public DbSet<Buyer> Buyers { get; set; }
    /// <summary>Таблица sellers</summary>
    public DbSet<Seller> Sellers { get; set; }
    /// <summary>Таблица currencies</summary>
    public DbSet<Currency> Currencies { get; set; }
    /// <summary>Таблица orders</summary>
    public DbSet<Order> Orders { get; set; }
    /// <summary>Таблица order_status_history</summary>
    public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging();
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Автоматически подключаем все IEntityTypeConfiguration<T>
        // из этой сборки (см. папку Configurations).
        modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
    }
}

