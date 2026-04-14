using DCAFlow.Contracts.Documents;
using DCAFlow.Contracts.Models;
using DCAFlow.Data.Repositories;
using DCAFlow.Web.Services;
using DCAFlow.Web.Settings;
using DCAFlow.Web.Validators;
using LiteDB;
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
    mapper.Entity<DocumentBase>().Id(x => x.Id);
    mapper.Entity<PortfolioDocument>().Id(x => x.Id);
    mapper.Entity<TransactionDocument>().Id(x => x.Id);
    mapper.Entity<ExchangeRateDocument>().Id(x => x.Id);

    return new LiteDatabase(connectionString, mapper);
});

builder.Services.AddMemoryCache();

var coinGeckoSetting = builder.Configuration.GetSection("CoinGecko").Get<CoinGeckoSettings>();
builder.Services.AddSingleton(coinGeckoSetting);

builder.Services.AddHttpClient(DCAFlow.Web.Constants.HttpClients.CoinGeckoClient, (_, client) =>
{
    client.BaseAddress = new Uri(coinGeckoSetting.BaseUrl);
});

builder.Services.AddScoped<TransactionRepository>();
builder.Services.AddScoped<PortfolioRepository>();
builder.Services.AddScoped<ExchangeRateRepository>();
builder.Services.AddScoped<CoinGeckoRateProvider>();
builder.Services.AddScoped<ExchangeRateProvider>();
builder.Services.AddScoped<PortfolioService>();
builder.Services.AddScoped<CoinService>();

builder.Services.AddControllers();

var app = builder.Build();

app.UseCors("AllowAll");

app.UseRouting();

app.MapStaticAssets();

app.MapControllers();

await app.RunAsync();
