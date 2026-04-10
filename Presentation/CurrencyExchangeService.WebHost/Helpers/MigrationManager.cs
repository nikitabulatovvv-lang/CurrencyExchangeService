using Microsoft.EntityFrameworkCore;

namespace CurrencyExchangeService.WebHost.Helpers;

public static class MigrationManager
{
    public static IHost MigrateDatabase(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetService<DbContext>();
        db?.Database.Migrate();
        return host;
    }
}

