﻿using System.Diagnostics.CodeAnalysis;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.SimpleEmail;
using form_builder.Attributes;
using form_builder.Cache;
using form_builder.Configuration;
using form_builder.ContentFactory.PageFactory;
using form_builder.ContentFactory.SuccessPageFactory;
using form_builder.Factories.Schema;
using form_builder.Factories.Transform.Lookups;
using form_builder.Factories.Transform.ReusableElements;
using form_builder.Gateways;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Helpers.DocumentCreation;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.IncomingDataHelper;
using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Helpers.ViewRender;
using form_builder.Mappers;
using form_builder.Providers;
using form_builder.Providers.Address;
using form_builder.Providers.Booking;
using form_builder.Providers.DocumentCreation;
using form_builder.Providers.DocumentCreation.Generic;
using form_builder.Providers.EmailProvider;
using form_builder.Providers.Organisation;
using form_builder.Providers.PaymentProvider;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Providers.Street;
using form_builder.Providers.ReferenceNumbers;
using form_builder.Providers.Transforms.Lookups;
using form_builder.Providers.Transforms.ReusableElements;
using form_builder.Services.AddressService;
using form_builder.Services.BookingService;
using form_builder.Services.DocumentService;
using form_builder.Services.EmailService;
using form_builder.Services.FileUploadService;
using form_builder.Services.MappingService;
using form_builder.Services.OrganisationService;
using form_builder.Services.PageService;
using form_builder.Services.PayService;
using form_builder.Services.RetrieveExternalDataService;
using form_builder.Services.StreetService;
using form_builder.Services.SubmitService;
using form_builder.Services.ValidateService;
using form_builder.TagParsers;
using form_builder.TagParsers.Formatters;
using form_builder.Validators;
using form_builder.Workflows.ActionsWorkflow;
using form_builder.Workflows.DocumentWorkflow;
using form_builder.Workflows.PaymentWorkflow;
using form_builder.Workflows.SubmitWorkflow;
using form_builder.Workflows.SuccessWorkflow;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using StockportGovUK.NetStandard.Gateways;
using StockportGovUK.NetStandard.Gateways.AddressService;
using StockportGovUK.NetStandard.Gateways.BookingService;
using StockportGovUK.NetStandard.Gateways.CivicaPay;
using StockportGovUK.NetStandard.Gateways.Extensions;
using StockportGovUK.NetStandard.Gateways.OrganisationService;
using StockportGovUK.NetStandard.Gateways.StreetService;
using StockportGovUK.NetStandard.Gateways.VerintService;

namespace form_builder.Utils.ServiceCollectionExtensions
{
    [ExcludeFromCodeCoverage]
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddTransient<IElementValidator, RequiredElementValidator>();
            services.AddTransient<IElementValidator, MultipleFileUploadElementValidator>();
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
            services.AddTransient<IElementValidator, RestrictMimeTypeValidator>();
            services.AddTransient<IElementValidator, RestrictFileSizeValidator>();
            services.AddTransient<IElementValidator, RestrictCombinedFileSizeValidator>();
            services.AddTransient<IElementValidator, BookingValidator>();
            services.AddTransient<IElementValidator, StreetSearchValidator>();
            services.AddTransient<IElementValidator, IsDateBeforeAbsoluteValidator>();
            services.AddTransient<IElementValidator, IsDateBeforeValidator>();
            services.AddTransient<IElementValidator, IsDateAfterAbsoluteValidator>();
            services.AddTransient<IElementValidator, IsDateAfterValidator>();

            return services;
        }

        public static IServiceCollection AddTagParsers(this IServiceCollection services)
        {
            services.AddTransient<ITagParser, FormAnswerTagParser>();
            services.AddTransient<ITagParser, FormDataTagParser>();
            services.AddTransient<ITagParser, LinkTagParser>();

            return services;
        }


        public static IServiceCollection AddGateways(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IS3Gateway, S3Gateway>();

            services.AddHttpClient<IGateway, Gateway>(configuration);
            services.AddHttpClient<ICivicaPayGateway, CivicaPayGateway>(configuration);
            services.AddHttpClient<IVerintServiceGateway, VerintServiceGateway>(configuration);
            services.AddHttpClient<IAddressServiceGateway, AddressServiceGateway>(configuration);
            services.AddHttpClient<IStreetServiceGateway, StreetServiceGateway>(configuration);
            services.AddHttpClient<IOrganisationServiceGateway, OrganisationServiceGateway>(configuration);
            services.AddHttpClient<IBookingServiceGateway, BookingServiceGateway>(configuration);

            return services;
        }

        public static IServiceCollection AddAmazonS3Client(this IServiceCollection services, string accessKey, string secretKey)
        {
            services.AddSingleton<IAmazonS3, AmazonS3Client>(provider => new AmazonS3Client(accessKey, secretKey, RegionEndpoint.EUWest1));

            return services;
        }

        public static IServiceCollection AddSesEmailConfiguration(this IServiceCollection services, string accessKey, string secretKey)
        {
            var credentials = new BasicAWSCredentials(accessKey, secretKey);
            services.AddTransient<IAmazonSimpleEmailService>(_ =>
                new AmazonSimpleEmailServiceClient(credentials, RegionEndpoint.EUWest1));

            return services;
        }

        public static IServiceCollection AddHelpers(this IServiceCollection services)
        {
            services.AddSingleton<IPageHelper, PageHelper>();
            services.AddSingleton<IElementHelper, ElementHelper>();
            services.AddSingleton<IElementMapper, ElementMapper>();
            services.AddSingleton<IDocumentCreationHelper, DocumentCreationHelper>();
            services.AddSingleton<IActionHelper, ActionHelper>();
            services.AddSingleton<IIncomingDataHelper, IncomingDataHelper>();

            services.AddHttpContextAccessor();
            services.AddScoped<IViewRender, ViewRender>();
            services.AddSingleton<ISessionHelper, SessionHelper>();

            return services;
        }

        public static IServiceCollection AddAttributes(this IServiceCollection services)
        {
            services.AddSingleton<ValidateReCaptchaAttribute>();

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

        public static IServiceCollection ConfigureBookingProviders(this IServiceCollection services)
        {
            services.AddSingleton<IBookingProvider, FakeBookingProvider>();
            services.AddSingleton<IBookingProvider, BookingProvider>();

            return services;
        }

        public static IServiceCollection ConfigureOrganisationProviders(this IServiceCollection services)
        {
            services.AddSingleton<IOrganisationProvider, FakeOrganisationProvider>();
            services.AddSingleton<IOrganisationProvider, ServiceOrganisationProvider>();

            return services;
        }
        
        public static IServiceCollection ConfigureFormAnswersProviders(this IServiceCollection services)
        {
            services.AddSingleton<IFormAnswersProvider, FormAnswersProvider>();

            return services;
        }

        public static IServiceCollection ConfigureStreetProviders(this IServiceCollection services)
        {
            services.AddSingleton<IStreetProvider, FakeStreetProvider>();
            services.AddSingleton<IStreetProvider, ServiceStreetProvider>();

            return services;
        }

        public static IServiceCollection ConfigureFormatters(this IServiceCollection services)
        {
            services.AddSingleton<IFormatter, FullDateFormatter>();
            services.AddSingleton<IFormatter, TimeOnlyFormatter>();

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

            return services;
        }

        public static IServiceCollection ConfigureEmailProviders(this IServiceCollection services, IWebHostEnvironment hostEnvironment)
        {
            if (hostEnvironment.IsEnvironment("local") || hostEnvironment.IsEnvironment("uitest"))
                services.AddSingleton<IEmailProvider, FakeEmailProvider>();
            else
                services.AddSingleton<IEmailProvider, AwsSesProvider>();

            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IAddressService, AddressService>();
            services.AddSingleton<IBookingService, BookingService>();
            services.AddSingleton<IPageService, PageService>();
            services.AddSingleton<IStreetService, StreetService>();
            services.AddSingleton<ISubmitService, SubmitService>();
            services.AddSingleton<IPayService, PayService>();
            services.AddSingleton<IOrganisationService, OrganisationService>();
            services.AddSingleton<IMappingService, MappingService>();
            services.AddSingleton<IFileUploadService, FileUploadService>();
            services.AddSingleton<IDocumentSummaryService, DocumentSummaryService>();
            services.AddSingleton<IRetrieveExternalDataService, RetrieveExternalDataService>();
            services.AddSingleton<IEmailService, EmailService>();
            services.AddSingleton<IValidateService, ValidateService>();

            return services;
        }

        public static IServiceCollection AddWorkflows(this IServiceCollection services)
        {
            services.AddSingleton<ISubmitWorkflow, SubmitWorkflow>();
            services.AddSingleton<IPaymentWorkflow, PaymentWorkflow>();
            services.AddSingleton<IDocumentWorkflow, DocumentWorkflow>();
            services.AddSingleton<IActionsWorkflow, ActionsWorkflow>();
            services.AddSingleton<ISuccessWorkflow, SuccessWorkflow>();

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

        public static IServiceCollection AddSchemaProvider(this IServiceCollection services, IWebHostEnvironment hostEnvironment)
        {
            if (hostEnvironment.IsEnvironment("local") || hostEnvironment.IsEnvironment("uitest"))
            {
                services.AddSingleton<ISchemaProvider, LocalFileSchemaProvider>();
            }
            else
            {
                services.AddSingleton<ISchemaProvider, S3FileSchemaProvider>();
            }

            return services;
        }

        public static IServiceCollection AddTransformDataProvider(this IServiceCollection services, IWebHostEnvironment hostEnvironment)
        {
            if (hostEnvironment.IsEnvironment("local") || hostEnvironment.IsEnvironment("uitest"))
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
            services.Configure<FormConfiguration>(configuration.GetSection("FormConfig"));
            services.Configure<CivicaPaymentConfiguration>(configuration.GetSection("PaymentConfiguration"));
            services.Configure<DistributedCacheExpirationConfiguration>(configuration.GetSection("DistributedCacheExpiration"));
            services.Configure<DistributedCacheConfiguration>(cacheOptions => cacheOptions.UseDistributedCache = configuration.GetValue<bool>("UseDistributedCache"));
            services.Configure<AwsSesKeysConfiguration>(configuration.GetSection("Ses"));
            services.Configure<ReCaptchaConfiguration>(configuration.GetSection("ReCaptchaConfiguration"));
            services.Configure<SubmissionServiceConfiguration>(configuration.GetSection("SubmissionServiceConfiguration"));
            services.Configure<TagManagerConfiguration>(TagManagerId => configuration.GetValue<string>("GoogleTagManagerId"));

            return services;
        }

        public static IServiceCollection AddCache(this IServiceCollection services)
        {
            services.AddTransient<ICache, Cache.Cache>();

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

        public static IServiceCollection AddReferenceNumberProvider(this IServiceCollection services)
        {
            services.AddTransient<IReferenceNumberProvider, ReferenceNumberProvider>();

            return services;
        }

        public static IServiceCollection AddRazorViewEngineViewLocations(this IServiceCollection services)
        {
            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add("/Views/Shared/Address/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/Booking/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/Breadcrumbs/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/ChangeSearch/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/Chevron/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/Cookie/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/DateInput/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/DatePicker/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/Declaration/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/Document/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/Error/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/FileUpload/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/Button/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/Footer/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/Header/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/Headings/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/Hr/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/Img/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/Inputs/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/Inputs/Checkbox/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/Inputs/Radio/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/Label/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/Legend/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/Link/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/Ol/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/P/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/Select/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/Textarea/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/Textbox/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/HtmlElements/Ul/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/IAG/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/Map/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/Numeric/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/Organisation/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/Payment/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/Recaptcha/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/Street/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/Tagmanager/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/Time/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/TimeInput/{0}" + RazorViewEngine.ViewExtension);
                o.ViewLocationFormats.Add("/Views/Shared/Warning/{0}" + RazorViewEngine.ViewExtension);
            });

            return services;
        }
    }
}