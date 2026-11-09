using System.Collections.Concurrent;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace ApiSecurityPlatform.Middlewares;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    
    // IP adreslerinin istek sayılarını ve zaman aralıklarını hafızada tutuyoruz
    private static readonly ConcurrentDictionary<string, (int Count, DateTime WindowStart)> _clients = new();

    // Güvenlik Politikası: 10 saniye içinde maksimum 5 istek hakkı
    private const int RequestLimit = 5;
    private static readonly TimeSpan TimeWindow = TimeSpan.FromSeconds(10);

    public RateLimitingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var now = DateTime.UtcNow;

        var clientState = _clients.AddOrUpdate(
            clientIp,
            _ => (1, now),
            (_, current) =>
            {
                // 10 saniyelik zaman penceresi dolduysa sayacı sıfırla
                if (now - current.WindowStart > TimeWindow)
                {
                    return (1, now);
                }
                // Zaman penceresi içerisindeyse sayacı 1 artır
                return (current.Count + 1, current.WindowStart);
            });

        // Belirlenen limit aşıldıysa isteği kes ve Güvenlik Olayı kaydet
        if (clientState.Count > RequestLimit)
        {
            var securityEvent = new SecurityEvent
            {
                EventType = "RATE_LIMIT_EXCEEDED",
                Description = $"IP: {clientIp} adresi {context.Request.Path} yoluna 10 saniye içinde {clientState.Count}. isteği atarak limit aştı.",
                CreatedAt = DateTime.UtcNow
            };

            // Supabase üzerindeki SecurityEvents tablosuna kaydet
            dbContext.SecurityEvents.Add(securityEvent);
            await dbContext.SaveChangesAsync();

            // İstemciye HTTP 429 Dön
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\": \"Çok fazla istek gönderildi. Lütfen 10 saniye bekleyip tekrar deneyin.\"}");
            
            return; // İsteği Controller/Endpoint katmanına geçirmeden burada kesiyoruz
        }

        await _next(context);
    }
}