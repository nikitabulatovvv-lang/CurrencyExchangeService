using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CurrencyExchangeService.Domain.Entities;
using CurrencyExchangeService.Domain.Enums;

namespace CurrencyExchangeService.Infrastructure.EntityFramework.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("order_status_history");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property<Guid>("OrderId")
            .HasColumnName("order_id")
            .IsRequired();

        builder.Property(x => x.OldStatus)
            .HasColumnName("old_status")
            .IsRequired()
            .HasConversion(
                s => s == OrderStatus.Active
                    ? "active"
                    : s == OrderStatus.Cancelled
                        ? "cancelled"
                        : "completed",
                str => Enum.Parse<OrderStatus>(str, ignoreCase: true)
            );

        builder.Property(x => x.NewStatus)
            .HasColumnName("new_status")
            .IsRequired()
            .HasConversion(
                s => s == OrderStatus.Active
                    ? "active"
                    : s == OrderStatus.Cancelled
                        ? "cancelled"
                        : "completed",
                str => Enum.Parse<OrderStatus>(str, ignoreCase: true)
            );

        builder.Property(x => x.ChangedAt)
            .HasColumnName("changed_at")
            .IsRequired()
            .HasDefaultValueSql("now()")
            .HasConversion(
                src => src.Kind == DateTimeKind.Utc ? src : DateTime.SpecifyKind(src, DateTimeKind.Utc),
                dst => dst.Kind == DateTimeKind.Utc ? dst : DateTime.SpecifyKind(dst, DateTimeKind.Utc)
            );

        builder.HasOne(x => x.Order)
            .WithMany("_statusHistory")
            .HasForeignKey("OrderId")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

