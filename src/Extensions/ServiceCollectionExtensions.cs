using Amazon;
using Amazon.S3;
using form_builder.Configuration;
using form_builder.Gateways;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Mappers;
using form_builder.Providers.Address;
using form_builder.Providers.PaymentProvider;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Services.AddressService;
using form_builder.Services.MappingService;
using form_builder.Services.OrganisationService;
using form_builder.Services.PageService;
using form_builder.Services.PayService;
using form_builder.Services.StreetService;
using form_builder.Services.SubmtiService;
using form_builder.Validators;
using form_builder.Workflows;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using form_builder.Services.FileUploadService;
using form_builder.Providers.DocumentCreation;
using form_builder.Providers.DocumentCreation.Generic;
using form_builder.Services.DocumentService;
using form_builder.Helpers.DocumentCreation;
using form_builder.ContentFactory;
using form_builder.Providers.Organisation;
using form_builder.Providers.Street;
using form_builder.Factories.Schema;
using form_builder.Providers.Transforms.Lookups;
using form_builder.Providers.Transforms.ReusableElements;
using form_builder.Factories.Transform.Lookups;
using form_builder.Factories.Transform.ReusableElements;
using form_builder.Services.ActionsService;

namespace form_builder.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddTransient<IElementValidator, RequiredElementValidator>();
            services.AddTransient<IElementValidator, NumericValueValidator>();
            services.AddTransient<IElementValidator, AutomaticAddressElementValidator>();
            services.AddTransient<IElementValidator, ManualAddressValidator>();
            services.AddTransient<IElementValidator, DateInputElementValidator>();
            services.AddTransient<IElementValidator, DatePickerElementValidator>();
            services.AddTransient<IElementValidator, RestrictPastDateValidator>();
            services.AddTransient<IElementValidator, RestrictFutureDateValidator>();
            services.AddTransient<IElementValidator, RestrictCurrentDateValidator>();
            services.AddTransient<IElementValidator, RestrictCurrentDatepickerValidator>();
            services.AddTransient<IElementValidator, RestrictPastDatepickerValidator>();
            services.AddTransient<IElementValidator, RestrictFutureDatepickerValidator>();
            services.AddTransient<IElementValidator, EmailElementValidator>();
            services.AddTransient<IElementValidator, PostcodeElementValidator>();
            services.AddTransient<IElementValidator, StockportPostcodeElementValidator>();
            services.AddTransient<IElementValidator, StockportAddressPostcodeElementValidator>();
            services.AddTransient<IElementValidator, RegexElementValidator>();
            services.AddTransient<IElementValidator, RequiredIfValidator>();
            services.AddTransient<IElementValidator, TimeInputValidator>();
            services.AddTransient<IElementValidator, MaxLengthValidator>();
            services.AddTransient<IElementValidator, AddressPostcodeValidator>();
            services.AddTransient<IElementValidator, RestrictFileSizeValidator>();
            services.AddTransient<IElementValidator, RestrictMimeTypeValidator>();

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
            services.AddSingleton<IElementMapper, ElementMapper>();
            services.AddSingleton<IDocumentCreationHelper, DocumentCreationHelper>();

            services.AddHttpContextAccessor();
            services.AddScoped<IViewRender, ViewRender>();
            services.AddSingleton<ISessionHelper, SessionHelper>();

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
        public static IServiceCollection ConfigureAddressProviders(this IServiceCollection services)
        {
            services.AddSingleton<IAddressProvider, FakeAddressProvider>();
            services.AddSingleton<IAddressProvider, ServiceAddressProvider>();
            return services;
        }

        public static IServiceCollection ConfigureOrganisationProviders(this IServiceCollection services)
        {
            services.AddSingleton<IOrganisationProvider, FakeOrganisationProvider>();
            services.AddSingleton<IOrganisationProvider, ServiceOrganisationProvider>();
            
            return services;
        }

        public static IServiceCollection ConfigureStreetProviders(this IServiceCollection services)
        {
            services.AddSingleton<IStreetProvider, FakeStreetProvider>();
            services.AddSingleton<IStreetProvider, ServiceStreetProvider>();
            
            return services;
        }

        public static IServiceCollection ConfigurePaymentProviders(this IServiceCollection services)
        {
            services.AddSingleton<IPaymentProvider, CivicaPayProvider>();
            return services;
        }

        public static IServiceCollection ConfigureDocumentCreationProviders(this IServiceCollection services)
        {
            services.AddSingleton<IDocumentCreation, TextfileDocumentCreator>();
            //services.AddSingleton<IDocumentCreation, SmbcTextfileDocumentCreator>();
            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IAddressService, AddressService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<IStreetService, StreetService>();
            services.AddSingleton<ISubmitService, SubmitService>();
            services.AddSingleton<IPayService, PayService>();
            services.AddSingleton<IOrganisationService, OrganisationService>();
            services.AddSingleton<IMappingService, MappingService>();
            services.AddSingleton<IFileUploadService, FileUploadService>();
            services.AddSingleton<IDocumentSummaryService, DocumentSummaryService>();
            services.AddSingleton<IActionsService, ActionsService>();

            return services;
        }

        public static IServiceCollection AddWorkflows(this IServiceCollection services)
        {
            services.AddSingleton<ISubmitWorkflow, SubmitWorkflow>();
            services.AddSingleton<IPaymentWorkflow, PaymentWorkflow>();
            services.AddSingleton<IDocumentWorkflow, DocumentWorkflow>();

            return services;
        }

        public static IServiceCollection AddFactories(this IServiceCollection services)
        {
            services.AddTransient<ISuccessPageFactory, SuccessPageFactory>();
            services.AddTransient<IPageFactory, PageFactory>();
            services.AddTransient<ISchemaFactory, SchemaFactory>();
            services.AddTransient<ILookupSchemaTransformFactory, LookupSchemaTransformFactory>();
            services.AddTransient<IReusableElementSchemaTransformFactory, ReusableElementSchemaTransformFactory>();

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

        public static IServiceCollection AddTransformDataProvider(this IServiceCollection services, IHostingEnvironment HostingEnvironment)
        {
            if (HostingEnvironment.IsEnvironment("local") || HostingEnvironment.IsEnvironment("uitest"))
            {
                services.AddSingleton<ILookupTransformDataProvider, LocalLookupTransformDataProvider>();
                services.AddSingleton<IReusableElementTransformDataProvider, LocalReusableElementTransformDataProvider>();
            }
            else
            {
                services.AddSingleton<ILookupTransformDataProvider, S3LookupTransformDataProvider>();
                services.AddSingleton<IReusableElementTransformDataProvider, S3ReusableElementTransformDataProvider>();

            }

            return services;
        }
        
        public static IServiceCollection AddIOptionsConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DisallowedAnswerKeysConfiguration>(configuration.GetSection("FormConfig"));
            services.Configure<CivicaPaymentConfiguration>(configuration.GetSection("PaymentConfiguration"));
            services.Configure<DistributedCacheExpirationConfiguration>(configuration.GetSection("DistrbutedCacheExpiration"));
            services.Configure<DistrbutedCacheConfiguration>(cacheOptions => cacheOptions.UseDistrbutedCache = configuration.GetValue<bool>("UseDistrbutedCache"));

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

                    var redis = ConnectionMultiplexer.Connect(storageProviderConfiguration["Address"]);
                    services.AddDataProtection().PersistKeysToStackExchangeRedis(redis, $"{storageProviderConfiguration["InstanceName"]}DataProtection-Keys");
                    break;

                case "Application":
                    services.AddDistributedMemoryCache();
                    break;

                default:
                    services.AddDistributedMemoryCache();
                    break;
            }

            services.AddDataProtection().SetApplicationName("formbuilder");
            services.AddSingleton<IDistributedCacheWrapper, DistributedCacheWrapper>();
            return services;
        }
    }
}
