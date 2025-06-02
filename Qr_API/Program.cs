using Microsoft.EntityFrameworkCore;
using Qr_API;
using Qr_API.DbContext;
using Qr_API.Queries;
using Qr_API.Requests;
using Qr_API.Factories;
using Qr_API.Observers;
using Qr_API.Services;
using Qr_API.Decorators;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<QrDbContext>(options =>
    options.UseSqlite("Data Source=qr.db"));

// Реєструємо всі сервіси
builder.Services.AddScoped<IMediator, Mediator>();
builder.Services.AddScoped<IQrGenerationService, QrGenerationService>();
builder.Services.AddScoped<IQrStrategyFactory, QrStrategyFactory>();

// Реєструємо спостерігачів
builder.Services.AddScoped<IQrProcessingObserver, EmailNotificationObserver>();
builder.Services.AddScoped<IQrProcessingObserver, AnalyticsObserver>();
builder.Services.AddScoped<IQrProcessingObserver, AuditObserver>();

// Налаштовуємо основну стратегію з декораторами
builder.Services.AddScoped<IQrStrategy>(provider =>
{
    var factory = provider.GetRequiredService<IQrStrategyFactory>();
    var logger1 = provider.GetRequiredService<ILogger<LoggingQrStrategyDecorator>>();
    var logger2 = provider.GetRequiredService<ILogger<CachingQrStrategyDecorator>>();
    var logger3 = provider.GetRequiredService<ILogger<ValidationQrStrategyDecorator>>();

    var baseStrategy = factory.CreateStrategy(QrStrategyType.Advanced);

    // Тут будуть декоратори, коли їх буде створено
    return baseStrategy;
});

// Налаштовуємо фасад
builder.Services.AddScoped<IQrFacade>((provider) =>
{
    var strategy = provider.GetRequiredService<IQrStrategy>();
    var db = provider.GetRequiredService<QrDbContext>();
    var logger = provider.GetRequiredService<ILogger<QrFacade>>();
    var observers = provider.GetServices<IQrProcessingObserver>();

    var facade = new QrFacade(strategy, db, logger);

    foreach (var observer in observers)
    {
        facade.Subscribe(observer);
    }

    return facade;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Основні endpoints
app.MapPost("/api/qr", async (QrRequest request, IMediator mediator) =>
{
    var result = await mediator.Send(new ProcessQrCommand(request));
    return Results.Ok(result);
});

app.MapGet("/api/qr/{id:int}", async (int id, IMediator mediator) =>
{
    var qr = await mediator.Send(new GetQrByIdQuery(id));
    return qr is not null ? Results.Ok(qr) : Results.NotFound();
});

// Новий endpoint для отримання QR-зображення
app.MapGet("/api/qr/{id:int}/image", async (int id, IQrFacade facade) =>
{
    var imageBytes = await facade.GetQrImageById(id);
    if (imageBytes == null)
        return Results.NotFound("QR code image not found");

    return Results.File(imageBytes, "image/png", $"qr_{id}.png");
});

// Endpoint з вибором стратегії
app.MapPost("/api/qr/strategy/{strategyType}", async (QrRequest request, string strategyType, IQrStrategyFactory factory) =>
{
    if (!Enum.TryParse<QrStrategyType>(strategyType, true, out var type))
    {
        return Results.BadRequest($"Invalid strategy type: {strategyType}. Valid types: {string.Join(", ", Enum.GetNames<QrStrategyType>())}");
    }

    var strategy = factory.CreateStrategy(type);
    var result = strategy.Process(request);
    return Results.Ok(new { StrategyUsed = strategyType, Result = result });
});

// Новий endpoint для прямого завантаження QR-коду
app.MapPost("/api/qr/generate", async (QrRequest request, IQrGenerationService qrService) =>
{
    try
    {
        var qrImageBase64 = qrService.GenerateQrCodeBase64(request.Data, request.Size, request.Format);
        
        if (request.Format == QrFormat.Base64)
        {
            return Results.Ok(new { QrCode = qrImageBase64, Format = request.Format.ToString() });
        }

        var imageBytes = Convert.FromBase64String(qrImageBase64);
        var contentType = request.Format == QrFormat.SVG ? "image/svg+xml" : "image/png";
        var fileName = $"qr.{request.Format.ToString().ToLower()}";
        
        return Results.File(imageBytes, contentType, fileName);
    }
    catch (Exception ex)
    {
        return Results.BadRequest($"Error generating QR code: {ex.Message}");
    }
});

// Endpoint для списку всіх QR-кодів
app.MapGet("/api/qr", async (QrDbContext context, int page = 1, int pageSize = 10) =>
{
    var qrCodes = await context.QrCodes
        .OrderByDescending(q => q.CreatedAt)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(q => new
        {
            q.Id,
            q.Data,
            q.Processed,
            q.CreatedAt,
            q.StrategyUsed,
            q.ErrorMessage,
            HasImage = !string.IsNullOrEmpty(q.QrImageBase64)
        })
        .ToListAsync();

    var total = await context.QrCodes.CountAsync();

    return Results.Ok(new
    {
        QrCodes = qrCodes,
        Total = total,
        Page = page,
        PageSize = pageSize,
        TotalPages = (int)Math.Ceiling(total / (double)pageSize)
    });
});

app.Run();