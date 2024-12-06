namespace form_builder.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

public class SessionLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionLoggingMiddleware> _logger;

    public SessionLoggingMiddleware(RequestDelegate next, ILogger<SessionLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/assets") && !context.Request.Path.StartsWithSegments("/_healthcheck"))
        {      
            if (context.Session is not null && !context.Session.IsAvailable)
                _logger.LogInformation("SessionLoggingMiddleware::Session is not available.");
        }

        await _next(context);
    }
}
