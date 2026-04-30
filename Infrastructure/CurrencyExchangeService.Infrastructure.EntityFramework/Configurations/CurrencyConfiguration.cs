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
    }
}

