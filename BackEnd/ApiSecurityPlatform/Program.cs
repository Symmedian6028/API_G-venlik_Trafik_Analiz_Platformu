using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Servis Kayıtları & CORS Politikası
// 1. Servis Kayıtları & CORS Politikası
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // Geçici ağ kopmalarında otomatik 3 kez tekrar dener
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(3),
            errorCodesToAdd: null);
    }));

var app = builder.Build();

// 2. CORS ve Middleware Sıralaması
app.UseCors("AllowFrontend");
app.UseMiddleware<ApiSecurityPlatform.Middlewares.TrafficLoggingMiddleware>();
app.UseMiddleware<ApiSecurityPlatform.Middlewares.RateLimitingMiddleware>();

// 3. Test Endpoints
app.MapGet("/api/status", () => "Sistem çalışıyor!");
app.MapGet("/test", () => Results.Ok(new { Message = "API is running", Time = DateTime.UtcNow }));

// 4. Dashboard Veri Endpoints (React Paneli İçin)
app.MapGet("/api/dashboard/logs", async (AppDbContext dbContext) =>
{
    var logs = await dbContext.RequestLogs
        .OrderByDescending(l => l.Timestamp)
        .Take(50)
        .ToListAsync();
    return Results.Ok(logs);
});

app.MapGet("/api/dashboard/security-events", async (AppDbContext dbContext) =>
{
    var events = await dbContext.SecurityEvents
        .OrderByDescending(e => e.CreatedAt)
        .Take(50)
        .ToListAsync();
    return Results.Ok(events);
});

// 5. Uygulamayı Başlat
app.Run();