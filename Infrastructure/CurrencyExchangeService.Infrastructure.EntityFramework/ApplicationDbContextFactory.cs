using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace CurrencyExchangeService.Infrastructure.EntityFramework;

/// <summary>
/// Design-time фабрика для <c>dotnet ef migrations</c> / <c>dotnet ef database update</c>.
/// Строка подключения: переменная <c>POSTGRES_CONNECTION_STRING</c> или
/// <c>ConnectionStrings:ApplicationDbContext</c> из <c>appsettings.json</c> проекта WebHost
/// (как у преподавателя для Notes/Shop: конфиг в JSON, при отсутствии — исключение).
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = ResolveConnectionString();

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString, options =>
        {
            options.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.GetName().Name);
        });

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string ResolveConnectionString()
    {
        var fromEnv = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(fromEnv))
            return fromEnv;

        var webHostDir = ResolveWebHostDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(webHostDir)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
            .Build();

        var connectionString = configuration.GetConnectionString(nameof(ApplicationDbContext));
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"В appsettings.json (папка WebHost: {webHostDir}) не задана строка подключения " +
                $"ConnectionStrings:{nameof(ApplicationDbContext)}. " +
                "Либо задайте её в JSON, либо переменную окружения POSTGRES_CONNECTION_STRING.");
        }

        return connectionString;
    }

    /// <summary>
    /// Ищет каталог <c>Presentation/CurrencyExchangeService.WebHost</c> с <c>appsettings.json</c>,
    /// поднимаясь от текущей рабочей директории (как при запуске <c>dotnet ef</c> из корня решения).
    /// Дополнительно: переменная <c>CURRENCY_EXCHANGE_WEBHOST_PATH</c> — абсолютный путь к папке WebHost.
    /// </summary>
    private static string ResolveWebHostDirectory()
    {
        var explicitPath = Environment.GetEnvironmentVariable("CURRENCY_EXCHANGE_WEBHOST_PATH");
        if (!string.IsNullOrWhiteSpace(explicitPath))
        {
            var trimmed = explicitPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (File.Exists(Path.Combine(trimmed, "appsettings.json")))
                return trimmed;
        }

        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir is not null)
        {
            var nested = Path.Combine(dir.FullName, "Presentation", "CurrencyExchangeService.WebHost", "appsettings.json");
            if (File.Exists(nested))
                return Path.GetDirectoryName(nested)!;

            if (string.Equals(dir.Name, "CurrencyExchangeService.WebHost", StringComparison.OrdinalIgnoreCase)
                && File.Exists(Path.Combine(dir.FullName, "appsettings.json")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException(
            "Не найден файл appsettings.json для WebHost. Запускайте команды EF из корня решения " +
            "(где есть папка Presentation/CurrencyExchangeService.WebHost), либо задайте " +
            "CURRENCY_EXCHANGE_WEBHOST_PATH на каталог с appsettings.json, либо POSTGRES_CONNECTION_STRING.");
    }
}
