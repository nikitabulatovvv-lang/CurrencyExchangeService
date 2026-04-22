using CurrencyExchangeService.Domain.Entities;
using CurrencyExchangeService.Domain.Enums;
using CurrencyExchangeService.ValueObjects;

namespace DomainApp;

internal class Program
{
    private static readonly List<Currency> Currencies = [];
    private static readonly List<Buyer> Buyers = [];
    private static readonly List<Seller> Sellers = [];
    private static readonly List<Order> Orders = [];
    private static readonly List<HistoryItem> History = [];

    private static void Main(string[] args)
    {
        SeedDemoData();

        while (true)
        {
            ShowMainMenu();
            var choice = Console.ReadLine()?.Trim();

            try
            {
                switch (choice)
                {
                    case "1":
                        EnterBuyerMode();
                        break;
                    case "2":
                        EnterSellerMode();
                        break;
                    case "3":
                        CreateBuyer();
                        break;
                    case "4":
                        CreateSeller();
                        break;
                    case "5":
                        CreateCurrency();
                        break;
                    case "6":
                        ShowHistory();
                        break;
                    case "0":
                        return;
                    default:
                        Console.WriteLine("Неизвестная команда.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Нажмите Enter для продолжения...");
            Console.ReadLine();
            Console.Clear();
        }
    }

    private static void ShowMainMenu()
    {
        Console.WriteLine("===== CurrencyExchange DomainApp =====");
        Console.WriteLine("1  - Войти как покупатель");
        Console.WriteLine("2  - Войти как продавец");
        Console.WriteLine("3  - Зарегистрировать покупателя");
        Console.WriteLine("4  - Зарегистрировать продавца");
        Console.WriteLine("5  - Добавить валюту");
        Console.WriteLine("6  - История выполненных/отмененных заявок");
        Console.WriteLine("0  - Выход");
        Console.Write("Выбор: ");
    }

    private static void SeedDemoData()
    {
        var usd = new Currency(new CurrencyCode("USD"));
        var eur = new Currency(new CurrencyCode("EUR"));
        var rub = new Currency(new CurrencyCode("RUB"));
        Currencies.AddRange([usd, eur, rub]);

        Buyers.Add(new Buyer(new Name("Fedor")));
        Sellers.Add(new Seller(new Name("Evgeny")));
    }

    private static void CreateCurrency()
    {
        Console.Write("Код валюты (например USD): ");
        var code = Console.ReadLine() ?? string.Empty;

        var currency = new Currency(new CurrencyCode(code));
        Currencies.Add(currency);
        Console.WriteLine($"Валюта создана: {currency.Id} ({currency.Code})");
    }

    private static void CreateBuyer()
    {
        Console.Write("Имя покупателя: ");
        var name = Console.ReadLine() ?? string.Empty;

        var buyer = new Buyer(new Name(name));
        Buyers.Add(buyer);
        Console.WriteLine($"Покупатель создан: {buyer.Id} ({buyer.Name})");
    }

    private static void CreateSeller()
    {
        Console.Write("Имя продавца: ");
        var name = Console.ReadLine() ?? string.Empty;

        var seller = new Seller(new Name(name));
        Sellers.Add(seller);
        Console.WriteLine($"Продавец создан: {seller.Id} ({seller.Name})");
    }

    private static void EnterBuyerMode()
    {
        var buyer = PickBuyer();
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"=== Покупатель: {buyer.Name} ===");
            Console.WriteLine("1 - Создать заявку на покупку");
            Console.WriteLine("2 - Мои активные заявки (изменить/отменить)");
            Console.WriteLine("3 - Активные заявки рынка (войти в сделку)");
            Console.WriteLine("4 - Назад в общее меню");
            Console.Write("Выбор: ");
            var choice = Console.ReadLine()?.Trim();
            try
            {
                switch (choice)
                {
                    case "1":
                        CreateBuyOrderForBuyer(buyer);
                        break;
                    case "2":
                        ManageBuyerOwnOrders(buyer);
                        break;
                    case "3":
                        BrowseMarketAsBuyer(buyer);
                        break;
                    case "4":
                        return;
                    default:
                        Console.WriteLine("Неизвестная команда.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Pause();
        }
    }

    private static void EnterSellerMode()
    {
        var seller = PickSeller();
        while (true)
        {
            Console.Clear();
            Console.WriteLine($"=== Продавец: {seller.Name} ===");
            Console.WriteLine("1 - Создать заявку на продажу");
            Console.WriteLine("2 - Мои активные заявки (изменить/отменить)");
            Console.WriteLine("3 - Активные заявки рынка (войти в сделку)");
            Console.WriteLine("4 - Назад в общее меню");
            Console.Write("Выбор: ");
            var choice = Console.ReadLine()?.Trim();
            try
            {
                switch (choice)
                {
                    case "1":
                        CreateSellOrderForSeller(seller);
                        break;
                    case "2":
                        ManageSellerOwnOrders(seller);
                        break;
                    case "3":
                        BrowseMarketAsSeller(seller);
                        break;
                    case "4":
                        return;
                    default:
                        Console.WriteLine("Неизвестная команда.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Pause();
        }
    }

    private static void CreateBuyOrderForBuyer(Buyer buyer)
    {
        var (baseCurrency, quoteCurrency) = PickCurrencyPair();
        var amount = ReadAmount(baseCurrency.Code.Value);
        var rate = ReadRate();

        var quoteAmount = amount.Value * rate.Value;
        Console.WriteLine($"Итог заявки: {amount.Value} {baseCurrency.Code} ≈ {quoteAmount:F6} {quoteCurrency.Code} по курсу {rate.Value}");

        var order = buyer.CreateBuyOrder(baseCurrency, quoteCurrency, amount, rate);
        Orders.Add(order);
        Console.WriteLine($"BUY-заявка создана: {order.Id}");
    }

    private static void CreateSellOrderForSeller(Seller seller)
    {
        var (baseCurrency, quoteCurrency) = PickCurrencyPair();
        var amount = ReadAmount(baseCurrency.Code.Value);
        var rate = ReadRate();

        var quoteAmount = amount.Value * rate.Value;
        Console.WriteLine($"Итог заявки: {amount.Value} {baseCurrency.Code} ≈ {quoteAmount:F6} {quoteCurrency.Code} по курсу {rate.Value}");

        var order = seller.CreateSellOrder(baseCurrency, quoteCurrency, amount, rate);
        Orders.Add(order);
        Console.WriteLine($"SELL-заявка создана: {order.Id}");
    }

    private static void ManageBuyerOwnOrders(Buyer buyer)
    {
        var ownOrders = Orders
            .Where(o => o.Buyer?.Id == buyer.Id && o.Status == OrderStatus.Active)
            .ToList();
        if (ownOrders.Count == 0)
        {
            Console.WriteLine("У вас нет активных заявок.");
            return;
        }

        Console.WriteLine("Ваши активные заявки:");
        var order = PickOrderFromList(ownOrders);
        PrintOrder(order);
        Console.WriteLine("1 - Изменить сумму/курс");
        Console.WriteLine("2 - Отменить заявку");
        Console.WriteLine("0 - Назад");
        Console.Write("Выбор: ");
        var action = Console.ReadLine()?.Trim();
        if (action == "1")
        {
            var newAmount = ReadAmount(order.BaseCurrency.Code.Value);
            var newRate = ReadRate();
            var changed = buyer.EditBuyOrder(order, newAmount, newRate);
            Console.WriteLine(changed ? "Заявка изменена." : "Данные не изменились.");
        }
        else if (action == "2")
        {
            var changed = buyer.CancelBuyOrder(order);
            Console.WriteLine(changed ? "Заявка отменена." : "Статус не изменился.");
            if (changed) AddHistory(order, "Покупатель отменил свою заявку", buyer.Name.Value, "—");
        }
    }

    private static void ManageSellerOwnOrders(Seller seller)
    {
        var ownOrders = Orders
            .Where(o => o.Seller?.Id == seller.Id && o.Status == OrderStatus.Active)
            .ToList();
        if (ownOrders.Count == 0)
        {
            Console.WriteLine("У вас нет активных заявок.");
            return;
        }

        Console.WriteLine("Ваши активные заявки:");
        var order = PickOrderFromList(ownOrders);
        PrintOrder(order);
        Console.WriteLine("1 - Изменить сумму/курс");
        Console.WriteLine("2 - Отменить заявку");
        Console.WriteLine("0 - Назад");
        Console.Write("Выбор: ");
        var action = Console.ReadLine()?.Trim();
        if (action == "1")
        {
            var newAmount = ReadAmount(order.BaseCurrency.Code.Value);
            var newRate = ReadRate();
            var changed = seller.EditSellOrder(order, newAmount, newRate);
            Console.WriteLine(changed ? "Заявка изменена." : "Данные не изменились.");
        }
        else if (action == "2")
        {
            var changed = seller.CancelSellOrder(order);
            Console.WriteLine(changed ? "Заявка отменена." : "Статус не изменился.");
            if (changed) AddHistory(order, "Продавец отменил свою заявку", "—", seller.Name.Value);
        }
    }

    private static void BrowseMarketAsBuyer(Buyer buyer)
    {
        var marketOrders = Orders
            .Where(o => o.Status == OrderStatus.Active && o.Buyer?.Id != buyer.Id)
            .ToList();
        if (marketOrders.Count == 0)
        {
            Console.WriteLine("Активных заявок для сделки нет.");
            return;
        }

        Console.WriteLine("Активные заявки рынка:");
        var order = PickOrderFromList(marketOrders);
        PrintOrder(order);
        Console.Write("Войти в сделку? (y/n): ");
        var agree = Console.ReadLine()?.Trim().ToLowerInvariant();
        if (agree == "y")
        {
            var changed = buyer.AcceptDeal(order);
            Console.WriteLine(changed ? "Сделка выполнена." : "Статус не изменился.");
            if (changed)
            {
                var historyBuyer = order.Type == OrderType.Sell ? buyer.Name.Value : order.Buyer?.Name.Value ?? "—";
                var historySeller = order.Type == OrderType.Sell ? order.Seller?.Name.Value ?? "—" : buyer.Name.Value;
                AddHistory(order, "Сделка выполнена", historyBuyer, historySeller);
            }
        }
    }

    private static void BrowseMarketAsSeller(Seller seller)
    {
        var marketOrders = Orders
            .Where(o => o.Status == OrderStatus.Active && o.Seller?.Id != seller.Id)
            .ToList();
        if (marketOrders.Count == 0)
        {
            Console.WriteLine("Активных заявок для сделки нет.");
            return;
        }

        Console.WriteLine("Активные заявки рынка:");
        var order = PickOrderFromList(marketOrders);
        PrintOrder(order);
        Console.Write("Войти в сделку? (y/n): ");
        var agree = Console.ReadLine()?.Trim().ToLowerInvariant();
        if (agree == "y")
        {
            var changed = seller.AcceptDeal(order);
            Console.WriteLine(changed ? "Сделка выполнена." : "Статус не изменился.");
            if (changed)
            {
                var historyBuyer = order.Type == OrderType.Sell ? seller.Name.Value : order.Buyer?.Name.Value ?? "—";
                var historySeller = order.Type == OrderType.Sell ? order.Seller?.Name.Value ?? "—" : seller.Name.Value;
                AddHistory(order, "Сделка выполнена", historyBuyer, historySeller);
            }
        }
    }

    private static Buyer PickBuyer()
    {
        if (Buyers.Count == 0) throw new InvalidOperationException("Нет покупателей.");
        Console.WriteLine("Покупатели:");
        for (var i = 0; i < Buyers.Count; i++)
            Console.WriteLine($"{i + 1}. {Buyers[i].Name} ({Buyers[i].Id})");
        Console.Write("Выберите покупателя: ");
        var index = ReadIndex(Buyers.Count);
        return Buyers[index];
    }

    private static Seller PickSeller()
    {
        if (Sellers.Count == 0) throw new InvalidOperationException("Нет продавцов.");
        Console.WriteLine("Продавцы:");
        for (var i = 0; i < Sellers.Count; i++)
            Console.WriteLine($"{i + 1}. {Sellers[i].Name} ({Sellers[i].Id})");
        Console.Write("Выберите продавца: ");
        var index = ReadIndex(Sellers.Count);
        return Sellers[index];
    }

    private static (Currency baseCurrency, Currency quoteCurrency) PickCurrencyPair()
    {
        if (Currencies.Count < 2) throw new InvalidOperationException("Нужно минимум 2 валюты.");

        Console.WriteLine("Валюты:");
        for (var i = 0; i < Currencies.Count; i++)
            Console.WriteLine($"{i + 1}. {Currencies[i].Code} ({Currencies[i].Id})");

        Console.Write("Выберите базовую валюту: ");
        var baseIdx = ReadIndex(Currencies.Count);
        Console.Write("Выберите котируемую валюту: ");
        var quoteIdx = ReadIndex(Currencies.Count);

        return (Currencies[baseIdx], Currencies[quoteIdx]);
    }

    private static Order PickOrderFromList(List<Order> list)
    {
        for (var i = 0; i < list.Count; i++)
        {
            Console.Write($"{i + 1}. ");
            PrintOrder(list[i]);
        }
        Console.Write("Выберите заявку: ");
        var idx = ReadIndex(list.Count);
        return list[idx];
    }

    private static Amount ReadAmount(string code)
    {
        Console.Write($"Сумма ({code}): ");
        var raw = Console.ReadLine() ?? string.Empty;
        if (!decimal.TryParse(raw, out var value))
            throw new FormatException("Некорректный формат суммы.");

        return new Amount(value);
    }

    private static Rate ReadRate()
    {
        Console.Write("Курс (rate): ");
        var raw = Console.ReadLine() ?? string.Empty;
        if (!decimal.TryParse(raw, out var value))
            throw new FormatException("Некорректный формат курса.");

        return new Rate(value);
    }

    private static int ReadIndex(int count)
    {
        var raw = Console.ReadLine() ?? string.Empty;
        if (!int.TryParse(raw, out var index) || index < 1 || index > count)
            throw new ArgumentOutOfRangeException(nameof(index), "Некорректный индекс.");
        return index - 1;
    }

    private static void PrintOrder(Order o)
    {
        var quoteAmount = o.Amount.Value * o.Rate.Value;
        var owner = o.Type == OrderType.Buy
            ? $"Buyer:{o.Buyer?.Name.Value}"
            : $"Seller:{o.Seller?.Name.Value}";

        Console.WriteLine(
            $"Id={o.Id} | {owner} | Type={o.Type} | {o.Amount.Value} {o.BaseCurrency.Code} (~ {quoteAmount:F6} {o.QuoteCurrency.Code}) | Rate={o.Rate.Value} | Status={o.Status}"
        );
    }

    private static void ShowHistory()
    {
        if (History.Count == 0)
        {
            Console.WriteLine("История пуста: выполненных или отмененных заявок пока нет.");
            return;
        }

        Console.WriteLine("=== История ===");
        foreach (var h in History.OrderByDescending(x => x.TimestampUtc))
        {
            Console.WriteLine(
                $"{h.TimestampUtc:u} | {h.Result} | Order={h.OrderId} | Buyer={h.BuyerName} | Seller={h.SellerName} | " +
                $"{h.BaseAmount} {h.BaseCode} -> {h.QuoteAmount:F6} {h.QuoteCode} | Rate={h.RateValue} | {h.Comment}"
            );
        }
    }

    private static void AddHistory(Order order, string comment, string buyerName, string sellerName)
    {
        var item = new HistoryItem(
            TimestampUtc: DateTime.UtcNow,
            OrderId: order.Id,
            Result: order.Status.ToString(),
            BuyerName: buyerName,
            SellerName: sellerName,
            BaseCode: order.BaseCurrency.Code.Value,
            QuoteCode: order.QuoteCurrency.Code.Value,
            BaseAmount: order.Amount.Value,
            QuoteAmount: order.Amount.Value * order.Rate.Value,
            RateValue: order.Rate.Value,
            Comment: comment
        );
        History.Add(item);
    }

    private static void Pause()
    {
        Console.WriteLine();
        Console.WriteLine("Нажмите Enter для продолжения...");
        Console.ReadLine();
    }

    private sealed record HistoryItem(
        DateTime TimestampUtc,
        Guid OrderId,
        string Result,
        string BuyerName,
        string SellerName,
        string BaseCode,
        string QuoteCode,
        decimal BaseAmount,
        decimal QuoteAmount,
        decimal RateValue,
        string Comment
    );
}

