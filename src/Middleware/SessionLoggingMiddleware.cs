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
        var session = context.Session;

        if (!session.IsAvailable)
        {
            _logger.LogInformation("SessionLoggingMiddleware::Session is not available.");
        }
        else
        {
            if (session.GetString("IsNewSession") == null)
            {
                _logger.LogInformation($"SessionLoggingMiddleware::A new session has been created:{session.Id} @ {DateTime.Now.ToString("HH:mm")}");
                session.SetString("IsNewSession", "false");
            }
        }

        await _next(context);
    }
}

public static class SessionLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseSessionLogging(this IApplicationBuilder builder) => builder.UseMiddleware<SessionLoggingMiddleware>();
}
