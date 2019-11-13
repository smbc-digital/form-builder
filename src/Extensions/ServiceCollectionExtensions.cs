using Amazon;
using Amazon.S3;
using form_builder.Configuration;
using form_builder.Gateways;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.PageHelpers;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Validators;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace form_builder.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddTransient<IElementValidator, RequiredElementValidator>();
            services.AddTransient<IElementValidator, NumericValueElementValidator>();
            services.AddTransient<IElementValidator, DateInputElementValidator>();

            return services;
        }

        public static IServiceCollection AddGateways(this IServiceCollection services)
        {
            services.AddSingleton<IS3Gateway, S3Gateway>();

            return services;
        }

        public static IServiceCollection AddAmazonS3Client(this IServiceCollection services, string accessKey, string secretKey)
        {
            services.AddSingleton<IAmazonS3, AmazonS3Client>(provider => new AmazonS3Client(accessKey, secretKey, RegionEndpoint.EUWest1));

            return services;
        }

        public static IServiceCollection AddHelpers(this IServiceCollection services)
        {
            services.AddSingleton<IPageHelper, PageHelper>();
            services.AddSingleton<IElementHelper, ElementHelper>();


            return services;
        }

        public static IServiceCollection ConfigureCookiePolicy(this IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            return services;
        }

        public static IServiceCollection AddSchemaProvider(this IServiceCollection services, IHostingEnvironment HostingEnvironment)
        {
            if (HostingEnvironment.IsEnvironment("local") || HostingEnvironment.IsEnvironment("uitest"))
            {
                services.AddSingleton<ISchemaProvider, LocalFileSchemaProvider>();
            }
            else
            {
                services.AddSingleton<ISchemaProvider, S3FileSchemaProvider>();
            }

            return services;
        }

        public static IServiceCollection AddIOptionsConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DisallowedAnswerKeysConfiguration>(configuration.GetSection("FormConfig"));
            return services;
        }

        public static IServiceCollection AddStorageProvider(this IServiceCollection services, IConfiguration configuration)
        {
              var storageProviderConfiguration = configuration.GetSection("StorageProvider");

            switch (storageProviderConfiguration["Type"])
            {
                case "Redis":
                    services.AddStackExchangeRedisCache(options => 
                    {
                        options.Configuration = storageProviderConfiguration["Address"];
                        options.InstanceName = storageProviderConfiguration["InstanceName"];
                    });
                    break;
                case "Application":
                    services.AddDistributedMemoryCache();
                    break;
                default:
                    services.AddDistributedMemoryCache();
                    break;
            }

            services.AddSingleton<IDistributedCacheWrapper, DistributedCacheWrapper>();
            return services;
        }
    }
}
