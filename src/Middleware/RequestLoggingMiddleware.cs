using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;

namespace form_builder.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        
        public async Task Invoke(HttpContext context)
        {
            var bodyContent = "";
            var request = context.Request;
            request.EnableRewind(); 
            using (StreamReader reader 
                    = new StreamReader(request.Body, Encoding.UTF8, true, 1024, true))
            {
                bodyContent = reader.ReadToEnd();
            }
            
            request.Body.Position = 0;

            _logger.LogWarning($"RequestLoggingMiddleware.Invoke - Received request with body {bodyContent}");

            await _next(context);
        }
    }
}
