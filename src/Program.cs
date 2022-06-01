using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using StockportGovUK.AspNetCore.Logging.Elasticsearch.Aws;

namespace form_builder
{
    [ExcludeFromCodeCoverage]
    public class Program
    {

        static string _formBuilderEnvironment = Environment.GetEnvironmentVariable("FORMBUILDER_ENVIRONMENT");

        public static IConfiguration Configuration { 
            get
            {               
                var appsettingPrefix = string.IsNullOrEmpty(_formBuilderEnvironment) 
                        ? $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}"
                        : $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.{_formBuilderEnvironment.ToLower()}";

                return new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("./Config/appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"./Config/{appsettingPrefix}.json", optional: false)
                        .AddJsonFile("./Config/Secrets/appsettings.secrets.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"./Config/Secrets/{appsettingPrefix}.secrets.json", optional: false)
                        .AddEnvironmentVariables()
                        .Build();    
            } 
        }

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .WriteToElasticsearchAws(Configuration)
                .CreateLogger();

            BuildHost(args).Run();
        }

        public static IHost BuildHost(string[] args) {
            var startupType = string.IsNullOrEmpty(_formBuilderEnvironment) 
                                ? "form_builder.Startup"
                                : $"form_builder.Startup{_formBuilderEnvironment}";

            Log.Logger.Debug($"Using Environment: {_formBuilderEnvironment}");
            Log.Logger.Debug($"Using Environment: {startupType}");

            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //webBuilder.UseStartup<Startup>();
                    webBuilder.UseStartup(Type.GetType(startupType));
                    webBuilder.UseConfiguration(Configuration);
                })
                .UseSerilog()
                .Build();
        }
    }
}
