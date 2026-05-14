using Microsoft.EntityFrameworkCore;

namespace CurrencyExchangeService.WebHost.Helpers;

/// <summary>
/// Как в NotesService: расширение для применения миграций при старте хоста.
/// В отличие от шаблона преподавателя, контекст запрашивается через <see cref="Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService{T}"/>,
/// иначе при <c>GetService</c> и null миграции не выполняются.
/// </summary>
public static class MigrationManager
{
    public static IHost MigrateDatabase<T>(this IHost host)
        where T : DbContext
    {
        using var scope = host.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<T>();
        context.Database.Migrate();

        return host;
    }
}
