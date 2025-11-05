using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Shared.Configuration;
using Shared.Common;

namespace Shared.Middleware;

/// <summary>
/// Middleware برای احراز هویت درخواست‌های Gateway
/// این Middleware فقط درخواست‌هایی که از Gateway می‌آیند را قبول می‌کند
/// </summary>
public class GatewayAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GatewayAuthMiddleware> _logger;
    private readonly string _gatewayApiKey;

    public GatewayAuthMiddleware(RequestDelegate next, IOptions<GatewayOptions> gatewayOptions, ILogger<GatewayAuthMiddleware> logger)
    {
        _next = next;
        _logger = logger;
        _gatewayApiKey = gatewayOptions.Value.ApiKey ?? string.Empty;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // در Development، دسترسی مستقیم را Allow می‌کنیم
        var environment = context.RequestServices.GetRequiredService<IHostEnvironment>();
        if (environment.IsDevelopment())
        {
            await _next(context);
            return;
        }

        // بررسی Header برای API Key
        var apiKey = context.Request.Headers[HeaderNames.GatewayApiKey].FirstOrDefault();

        if (string.IsNullOrEmpty(apiKey) || apiKey != _gatewayApiKey)
        {
            _logger.LogWarning("Unauthorized access attempt from {RemoteIp} to {Path}", context.Connection.RemoteIpAddress, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "Forbidden",
                message = "Direct access to micro-services is not allowed. Please use API Gateway.",
                gatewayUrl = "http://localhost:5000"
            }));
            
            return;
        }

        await _next(context);
    }
}

/// <summary>
/// Extension Method برای اضافه کردن Gateway Auth Middleware
/// </summary>
public static class GatewayAuthMiddlewareExtensions
{
    public static IApplicationBuilder UseGatewayAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GatewayAuthMiddleware>();
    }
}

