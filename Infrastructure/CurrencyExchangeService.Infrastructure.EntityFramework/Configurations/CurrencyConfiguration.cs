using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CurrencyExchangeService.Domain.Entities;
using CurrencyExchangeService.ValueObjects;
using CurrencyExchangeService.ValueObjects.Validators;

namespace CurrencyExchangeService.Infrastructure.EntityFramework.Configurations;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("currencies");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .IsRequired()
            .HasConversion(
                code => code.Value,
                str => new CurrencyCode(str)
            )
            .HasMaxLength(CurrencyCodeValidator.MAX_LENGTH);

        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasMany<Order>("_baseOrders")
            .WithOne(o => o.BaseCurrency)
            .HasForeignKey("BaseCurrencyId")
            .HasPrincipalKey(x => x.Id);

        builder.HasMany<Order>("_quoteOrders")
            .WithOne(o => o.QuoteCurrency)
            .HasForeignKey("QuoteCurrencyId")
            .HasPrincipalKey(x => x.Id);

        builder.Ignore(x => x.BaseOrders);
        builder.Ignore(x => x.QuoteOrders);
    }
}

