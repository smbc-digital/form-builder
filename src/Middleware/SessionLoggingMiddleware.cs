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
        if (context.Session is not null && !context.Session.IsAvailable)
        {
            _logger.LogInformation("SessionLoggingMiddleware::Session is not available.");
        }
        else
        {
            if (context.Session.GetString("IsNewSession") == null)
            {
                _logger.LogInformation($"SessionLoggingMiddleware::A new session has been created:{context.Session.Id} @ {DateTime.Now.ToString("HH:mm")}");
                context.Session.SetString("IsNewSession", "false");
            }
        }

        await _next(context);
    }
}