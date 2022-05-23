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
        public static IConfiguration Configuration { 
            get
            {
                // This would allow the publication of this too a different environment with by setting "FORMBUILDER_ENVIRONMENT=internal"
                var formBuilderEnvironment = Environment.GetEnvironmentVariable("FORMBUILDER_ENVIRONMENT");
                
                var appsettingPrefix = string.IsNullOrEmpty(formBuilderEnvironment) 
                        ? $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}"
                        : $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.{formBuilderEnvironment}";

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
            var formBuilderEnvironment = Environment.GetEnvironmentVariable("FORMBUILDER_ENVIRONMENT");
            var startupType = string.IsNullOrEmpty(formBuilderEnvironment) 
                                ? "form_builder.Startup"
                                : $"form_builder.Startup{Environment.GetEnvironmentVariable("FORMBUILDER_ENVIRONMENT")}";


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
