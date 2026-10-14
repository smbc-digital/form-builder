namespace form_builder.Middleware;

public class SessionLoggingMiddleware(RequestDelegate next, ILogger<SessionLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/assets") && !context.Request.Path.StartsWithSegments("/_healthcheck"))
        {      
            if (context.Session is not null && !context.Session.IsAvailable)
                logger.LogInformation("SessionLoggingMiddleware::Session is not available.");
        }

        await next(context);
    }
}
