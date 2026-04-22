using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CurrencyExchangeService.Domain.Entities;
using CurrencyExchangeService.Domain.Enums;
using CurrencyExchangeService.ValueObjects;

namespace CurrencyExchangeService.Infrastructure.EntityFramework.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasConversion(
                t => t == OrderType.Buy ? "buy" : "sell",
                str => str == "buy" ? OrderType.Buy : OrderType.Sell
            );

        // nullable in ERD: buyers.id / sellers.id are optional in orders.*?
        builder.Property<Guid?>("BuyerId").HasColumnName("buyer_id");
        builder.Property<Guid?>("SellerId").HasColumnName("seller_id");

        builder.Property<Guid>("BaseCurrencyId").HasColumnName("base_currency_id");
        builder.Property<Guid>("QuoteCurrencyId").HasColumnName("quote_currency_id");

        builder.Property(x => x.Amount)
            .HasColumnName("amount")
            .IsRequired()
            .HasConversion(
                amount => amount.Value,
                value => new Amount(value)
            )
            .HasPrecision(18, 2);

        builder.Property(x => x.Rate)
            .HasColumnName("rate")
            .IsRequired()
            .HasConversion(
                rate => rate.Value,
                value => new Rate(value)
            )
            .HasPrecision(18, 6);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .IsRequired()
            .HasConversion(
                s => s == OrderStatus.Active
                    ? "active"
                    : s == OrderStatus.Cancelled
                        ? "cancelled"
                        : "completed",
                str => Enum.Parse<OrderStatus>(str, ignoreCase: true)
            );

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired()
            .HasDefaultValueSql("now()")
            .HasConversion(
                src => src.Kind == DateTimeKind.Utc ? src : DateTime.SpecifyKind(src, DateTimeKind.Utc),
                dst => dst.Kind == DateTimeKind.Utc ? dst : DateTime.SpecifyKind(dst, DateTimeKind.Utc)
            );

        builder.HasOne(x => x.Buyer)
            .WithMany("_orders")
            .HasForeignKey("BuyerId")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Seller)
            .WithMany("_orders")
            .HasForeignKey("SellerId")
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.BaseCurrency)
            .WithMany()
            .HasForeignKey("BaseCurrencyId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.QuoteCurrency)
            .WithMany()
            .HasForeignKey("QuoteCurrencyId")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany<OrderStatusHistory>("_statusHistory")
            .WithOne(h => h.Order)
            .HasForeignKey("OrderId");

        builder.Ignore(x => x.StatusHistory);
    }
}

