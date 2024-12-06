using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using form_builder.Middleware;
using form_builder.ModelBinders.Providers;
using form_builder.Utils.Startup;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using StockportGovUK.AspNetCore.Middleware.App;

namespace form_builder
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IWebHostEnvironment HostingEnvironment { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-GB");
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-GB");
            services.AddControllersWithViews();
            services.AddRazorPages();

            services
                .AddRazorViewEngineViewLocations()
                .ConfigureCookiePolicy()
                .AddValidators()
                .AddTagParsers()
                .AddReferenceNumberProvider()
                .AddStorageProvider(Configuration)
                .AddFileStorageProvider(Configuration)
                .AddSchemaProvider(HostingEnvironment)
                .AddTransformDataProvider(HostingEnvironment)
                .AddAmazonS3Client(Configuration.GetSection("AmazonS3Configuration")["AccessKey"], Configuration.GetSection("AmazonS3Configuration")["SecretKey"])
                .AddSesEmailConfiguration(Configuration.GetSection("Ses")["Accesskey"], Configuration.GetSection("Ses")["Secretkey"])
                .AddGateways(Configuration)
                .AddGovUkServices(Configuration)
                .AddIOptionsConfiguration(Configuration)
                .AddUtilities()
                .AddAnalyticsProviders()
                .ConfigureAddressProviders()
                .ConfigureCorsPolicy()
                .ConfigureDynamicLookDataProviders()
                .ConfigureOrganisationProviders()
                .ConfigureStreetProviders()
                .ConfigureSubmitProviders()
                .ConfigurePaymentProviders(HostingEnvironment)
                .ConfigureBookingProviders()
                .ConfigureEmailTemplateProviders()
                .ConfigureDocumentCreationProviders()
                .ConfigureFormAnswersProviders()
                .ConfigureFormatters()
                .ConfigureEnabledFor()
                .ConfigureEmailProviders(HostingEnvironment)
                .AddHelpers()
                .AddSchemaIntegrityValidation()
                .AddAttributes()
                .AddServices()
                .AddWorkflows()
                .AddFactories()
                .AddFormAccessRestrictions()
                .AddAntiforgery(_ =>
                    {
                        _.Cookie.Name = ".formbuilder.antiforgery.v2";
                        _.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    })
                .AddSession(_ =>
                {
                    _.IdleTimeout = TimeSpan.FromMinutes(60);
                    _.Cookie.Path = "/";
                    _.Cookie.Name = ".formbuilder.v2";
                    _.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                });

            services
                .AddMvc()
                .AddMvcOptions(options =>
                {
                    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                    options.ModelBinderProviders.Insert(0, new CustomFormFileModelBinderProvider());
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsEnvironment("local") || env.IsEnvironment("uitest"))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseMiddleware<AppExceptionHandling>()
                    .UseHsts();
            }

            app.UseMiddleware<HeaderConfiguration>()
                .UseMiddleware<LegacyRedirect>()
                .UseSession()
                .UseMiddleware<SessionLoggingMiddleware>()
                .UseStaticFiles()
                .UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider(
                        Path.Combine(env.ContentRootPath, "fonts")),
                    RequestPath = "/fonts"
                })
                .UseRouting()
                .UseEndpoints(endpoints =>
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}")
            );
            
            app.UseResponseCaching();
            app.Use(async (context, next) =>
            {
                context.Response.GetTypedHeaders().CacheControl =
                    new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
                    {
                        NoCache = true,
                        NoStore = true
                    };

                await next();
            });
        }
    }
}