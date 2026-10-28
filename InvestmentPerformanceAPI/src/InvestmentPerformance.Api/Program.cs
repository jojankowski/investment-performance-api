using InvestmentPerformance.Api.Data;
using InvestmentPerformance.Api.Interfaces;
using InvestmentPerformance.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddHostedService<DatabaseInitializer>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IPerformanceCalculator, PerformanceCalculator>();
builder.Services.AddScoped<IInvestmentRepository, InvestmentRepository>();
builder.Services.AddScoped<IMarketPriceProvider, DatabaseMarketPriceProvider>();
builder.Services.AddScoped<IInvestmentService, InvestmentService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
