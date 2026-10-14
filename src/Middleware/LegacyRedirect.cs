namespace form_builder.Middleware;

public class LegacyRedirect(RequestDelegate next)
{
    public async Task Invoke(HttpContext context)
    {
        var url = context.Request.Path.ToString().ToLower();
        if (url.StartsWith("/v2/"))
            context.Response.Redirect($"{url.Substring(3)}{(context.Request.QueryString.HasValue ? context.Request.QueryString.Value : string.Empty)}");
        else
            await next(context);
    }
}