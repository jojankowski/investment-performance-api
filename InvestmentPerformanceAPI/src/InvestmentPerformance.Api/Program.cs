using System.Text.Json.Serialization;
using InvestmentPerformance.Api.Data;
using InvestmentPerformance.Api.Handlers;
using InvestmentPerformance.Api.Interfaces;
using InvestmentPerformance.Api.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Enums such as Term are written as "Short" rather than 0.
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

// Reads the connection string from the finished configuration, including any override a test host adds.
builder.Services.AddDbContext<AppDbContext>((services, options) =>
    options.UseSqlite(services.GetRequiredService<IConfiguration>().GetConnectionString("Default")));
builder.Services.AddHostedService<DatabaseInitializer>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<IInvestmentRepository, InvestmentRepository>();
builder.Services.AddScoped<IMarketPriceProvider, DatabaseMarketPriceProvider>();
builder.Services.AddSingleton<IPerformanceCalculator, PerformanceCalculator>();
builder.Services.AddScoped<IInvestmentService, InvestmentService>();

// Every error response, from a crash to an unknown URL, uses the standard problem details format.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// The OpenAPI document and the interactive API page are only for local development.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();

app.Run();