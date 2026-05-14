using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using CurrencyExchangeService.Infrastructure.EntityFramework;
using CurrencyExchangeService.WebHost.Helpers;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString(nameof(ApplicationDbContext));

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string for ApplicationDbContext is not configured.");
}

builder.Services.AddNpgsql<ApplicationDbContext>(connectionString, options =>
{
    options.MigrationsAssembly("CurrencyExchangeService.Infrastructure.EntityFramework");
});

builder.Services.AddSwaggerGen(
    c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Currency exchange service API",
            Description = "API for creating, viewing, storing, modifying, and cancelling currency exchange orders."
        });
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.MigrateDatabase<ApplicationDbContext>();

app.Run();
