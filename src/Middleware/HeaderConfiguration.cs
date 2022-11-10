﻿namespace form_builder.Middleware
{
    public class HeaderConfiguration
    {
        private readonly RequestDelegate _next;
        private IWebHostEnvironment _env { get; }

        public HeaderConfiguration(RequestDelegate next, IWebHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        // TODO: Move these header values to config!
        public async Task Invoke(HttpContext context)
        {
            var headers = context.Response.Headers;

            var testUrls = !_env.IsEnvironment("stage") || !_env.IsEnvironment("prod") ? "http://localhost https://localhost" : "";

            var queryString = context.Request.QueryString.ToString();
            
            if (queryString.Contains("utm_source=lagan") || context.Request.Cookies.ContainsKey("is_verint"))
            {
                headers["Content-Security-Policy"] = "frame-ancestors 'self' http://www.stockport.gov.uk https://www.stockport.gov.uk http://scnverintlive.stockport.gov.uk:8080 http://scnverinttest.stockport.gov.uk:8080";
                headers["X-Content-Security-Policy"] = "frame-ancestors 'self' http://www.stockport.gov.uk https://www.stockport.gov.uk http://scnverintlive.stockport.gov.uk:8080 http://scnverinttest.stockport.gov.uk:8080";
                context.Response.Cookies.Append("is_verint", "yes");
            }
            else
            {
                headers["Content-Security-Policy"] = $"frame-ancestors 'self' http://www.stockport.gov.uk https://www.stockport.gov.uk https://int-iag-stockportgov.smbcdigital.net/ https://qa-iag-stockportgov.smbcdigital.net/ https://stage-iag-stockportgov.smbcdigital.net/ {testUrls}";
                headers["X-Content-Security-Policy"] = $"frame-ancestors 'self' http://www.stockport.gov.uk https://www.stockport.gov.uk https://int-iag-stockportgov.smbcdigital.net https://qa-iag-stockportgov.smbcdigital.net/ https://stage-iag-stockportgov.smbcdigital.net/ {testUrls}";
            }

            headers["Access-Control-Allow-Origin"] = $"http://www.stockport.gov.uk https://www.stockport.gov.uk https://int-iag-stockportgov.smbcdigital.net https://qa-iag-stockportgov.smbcdigital.net/ https://stage-iag-stockportgov.smbcdigital.net/ {testUrls}";
            headers["X-Content-Type-Options"] = "nosniff";
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

            await _next(context);
        }
    }
}