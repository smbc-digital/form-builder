using System.Diagnostics.CodeAnalysis;
using Serilog;
using StockportGovUK.AspNetCore.Logging.Elasticsearch.Aws;

namespace form_builder
{
    [ExcludeFromCodeCoverage]
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("./Config/appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"./Config/appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty}.json", optional: false)
            .AddJsonFile("./Config/Secrets/appsettings.secrets.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"./Config/Secrets/appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty}.secrets.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .WriteToElasticsearchAws(Configuration)
                .CreateLogger();

            BuildHost(args).Run();
        }

        public static IHost BuildHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseConfiguration(Configuration);
                })
                .UseSerilog()
                .Build();
    }
}
