using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using form_builder.Cache;
using form_builder.Configuration;
using form_builder.Middleware;
using form_builder.ModelBinders.Providers;
using form_builder.Utils.ServiceCollectionExtensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                .ConfigureCookiePolicy()
                .AddValidators()
                .AddStorageProvider(Configuration)
                .AddSchemaProvider(HostingEnvironment)
                .AddTransformDataProvider(HostingEnvironment)
                .AddAmazonS3Client(Configuration.GetSection("AmazonS3Configuration")["AccessKey"], Configuration.GetSection("AmazonS3Configuration")["SecretKey"])
                .AddSesEmailConfiguration(Configuration.GetSection("Ses")["Accesskey"], Configuration.GetSection("Ses")["Secretkey"])
                .AddGateways(Configuration)
                .AddIOptionsConfiguration(Configuration)
                .ConfigureAddressProviders()
                .ConfigureOrganisationProviders()
                .ConfigureStreetProviders()
                .ConfigurePaymentProviders()
                .ConfigureDocumentCreationProviders()
                .ConfigureEmailProviders(HostingEnvironment)
                .AddHelpers()
                .AddServices()
                .AddWorkflows()
                .AddFactories()
                .AddAntiforgery(_ => _.Cookie.Name = ".formbuilder.antiforgery.v2")
                .AddSession(_ =>
                {
                    _.IdleTimeout = TimeSpan.FromMinutes(30);
                    _.Cookie.Path = "/";
                    _.Cookie.Name = ".formbuilder.v2";
                });

            services.AddTransient<ICache, Cache.Cache>();
            services.Configure<SubmissionServiceConfiguration>(Configuration.GetSection("SubmissionServiceConfiguration"));
            services.AddTransient<ITagManagerConfiguration, TagManagerConfiguration>();

            services.AddMvc()
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
                app.UseMiddleware<AppExceptionHandling>();
                app.UseHsts();
            }

            app.UseMiddleware<HeaderConfiguration>();

            app.UseSession();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}")
            );
        }
    }
}