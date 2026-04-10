using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CurrencyExchangeService.Domain.Entities;
using CurrencyExchangeService.ValueObjects;
using CurrencyExchangeService.ValueObjects.Validators;

namespace CurrencyExchangeService.Infrastructure.EntityFramework.Configurations;

public class BuyerConfiguration : IEntityTypeConfiguration<Buyer>
{
    public void Configure(EntityTypeBuilder<Buyer> builder)
    {
        builder.ToTable("buyers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasConversion(
                name => name.Value,
                str => new Name(str)
            )
            .HasMaxLength(NameValidator.MAX_LENGTH);

        builder.HasMany<Order>("_orders")
            .WithOne(x => x.Buyer)
            .HasForeignKey("BuyerId")
            .HasPrincipalKey(x => x.Id);

        builder.Ignore(x => x.Orders);
    }
}

