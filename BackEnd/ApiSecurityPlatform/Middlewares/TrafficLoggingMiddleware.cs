using System.Diagnostics;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Http;

namespace ApiSecurityPlatform.Middlewares;

public class TrafficLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public TrafficLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
{
    var stopwatch = Stopwatch.StartNew();

    await _next(context);

    stopwatch.Stop();

    try
    {
        var requestLog = new RequestLog
        {
            Endpoint = context.Request.Path,
            ClientIp = context.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1",
            StatusCode = context.Response.StatusCode,
            DurationMs = stopwatch.ElapsedMilliseconds,
            Timestamp = DateTime.UtcNow
        };

        dbContext.RequestLogs.Add(requestLog);
        await dbContext.SaveChangesAsync();
        Console.WriteLine($"[TRAFİK LOGLANDI] {context.Request.Path} - {context.Response.StatusCode}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[LOG KAYIT HATASI]: {ex.Message}");
    }
}
}