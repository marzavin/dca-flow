using DCAFlow.Core.Assets;
using DCAFlow.Core.Charts;
using DCAFlow.Core.Coins;
using DCAFlow.Core.ExchangeRates;
using DCAFlow.Core.Metrics;
using DCAFlow.Core.Transactions;
using DCAFlow.Data.Entities;
using DCAFlow.Data.Repositories;
using DCAFlow.Web.Providers;
using DCAFlow.Web.Settings;
using LiteDB;
using SideEffect.Data;
using System.Reflection;

Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var connectionString = builder.Configuration.GetConnectionString("LiteDB");

builder.Services.AddSingleton(sp =>
{
    var mapper = BsonMapper.Global;
    mapper.Entity<EntityBase>().Id(x => x.Id);
    mapper.Entity<TransactionEntity>().Id(x => x.Id);

    return new LiteDatabase(connectionString, mapper);
});

builder.Services.AddMemoryCache();

var coinGeckoSetting = builder.Configuration.GetSection("CoinGecko").Get<CoinGeckoSettings>();
builder.Services.AddSingleton(coinGeckoSetting);

builder.Services.AddHttpClient(DCAFlow.Web.Constants.HttpClients.CoinGeckoClient, (_, client) =>
{
    client.BaseAddress = new Uri(coinGeckoSetting.BaseUrl);
});

builder.Services.AddSingleton<IExchangeRateProvider, CoinGeckoRateProvider>();
builder.Services.AddSingleton<ExchangeRateStorage>();

builder.Services.AddScoped<TransactionRepository>();

builder.Services.AddSingleton<CoinService>();
builder.Services.AddScoped<AssetService>();
builder.Services.AddScoped<ChartService>();
builder.Services.AddScoped<MetricService>();
builder.Services.AddScoped<TransactionService>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseDefaultFiles();

app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseRouting();

app.MapStaticAssets();

app.MapControllers();

await app.RunAsync();
