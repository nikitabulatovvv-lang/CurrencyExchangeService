using CurrencyExchangeService.Domain.Entities;
using CurrencyExchangeService.Domain.Enums;
using CurrencyExchangeService.ValueObjects;

namespace DomainApp;

internal class Program
{
    private static void Main(string[] args)
    {
        // Проверяет
        // - создание заявок
        // - смена статуса
        // - фильтрация активных заявок

        var usd = new Currency(Guid.NewGuid(), new CurrencyCode("USD"));
        var rub = new Currency(Guid.NewGuid(), new CurrencyCode("RUB"));

        var seller = new Seller(new Name("Test Sell"));

        // Use case: "Создание заявки на продажу"
        var order1 = seller.CreateSellOrder(usd, rub, 100m, 90m);
        var order2 = seller.CreateSellOrder(usd, rub, 50m, 89.5m);

        // Use case: "Подтверждение сделки"
        // После подтверждения заявка становится Completed и НЕ считается активной.
        seller.ConfirmDeal(order1);

        // Use case: "Просмотр активных заявок"
        var activeOrders = seller.GetActiveOrders();

        Console.WriteLine("Active orders for seller:");
        foreach (var o in activeOrders)
        {
            Console.WriteLine($"Id={o.Id}, Type={o.Type}, Amount={o.Amount}, Rate={o.Rate}, Status={o.Status}");
        }
    }
}

