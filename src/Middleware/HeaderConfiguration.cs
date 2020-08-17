﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace form_builder.Middleware
{
    public class HeaderConfiguration
    {
        private readonly RequestDelegate _next;

        public HeaderConfiguration(RequestDelegate next)
        {
            _next = next;
        }

        // TODO: Move these header values to config!
        public async Task Invoke(HttpContext context)
        {
            var headers = context.Response.Headers;

            var queryString = context.Request.QueryString.ToString();
            if (queryString.Contains("utm_source=lagan") || context.Request.Cookies.ContainsKey("is_verint"))
            {
                headers["Content-Security-Policy"] = $"frame-ancestors 'self' http://www.stockport.gov.uk https://www.stockport.gov.uk http://scnverintlive.stockport.gov.uk:8080 http://scnverinttest.stockport.gov.uk:8080";
                headers["X-Content-Security-Policy"] = $"frame-ancestors 'self' http://www.stockport.gov.uk https://www.stockport.gov.uk http://scnverintlive.stockport.gov.uk:8080 http://scnverinttest.stockport.gov.uk:8080";
                context.Response.Cookies.Append("is_verint", "yes");
            }
            else
            {
                headers["Content-Security-Policy"] = $"frame-ancestors 'self' http://www.stockport.gov.uk https://www.stockport.gov.uk";
                headers["X-Content-Security-Policy"] = $"frame-ancestors 'self' http://www.stockport.gov.uk https://www.stockport.gov.uk";
            }

            headers["Access-Control-Allow-Origin"] = $"http://www.stockport.gov.uk https://www.stockport.gov.uk";
            headers["X-Content-Type-Options"] = "nosniff";
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

            await _next(context);
        }
    }
}
