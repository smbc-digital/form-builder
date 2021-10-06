using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace form_builder.Middleware
{
    public class LegacyRedirect
    {
        private readonly RequestDelegate _next;

        public LegacyRedirect(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context)
        {
            var url = context.Request.Path.ToString().ToLower();
            if (url.StartsWith("/v2/"))
                context.Response.Redirect(url.Substring(3));
            else
                await _next(context);
        }
    }
}