using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using form_builder.Extensions;
using form_builder.Configuration;
using StockportGovUK.AspNetCore.Middleware.App;
using StockportGovUK.NetStandard.Gateways;
using form_builder.Cache;
using form_builder.ModelBinders.Providers;
using System.Globalization;
using form_builder.Middleware;

namespace form_builder
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public IHostingEnvironment HostingEnvironment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            HostingEnvironment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-GB");
            services
                .ConfigureCookiePolicy()
                .AddValidators()
                .AddStorageProvider(Configuration)
                .AddSchemaProvider(HostingEnvironment)
                .AddTransformDataProvider(HostingEnvironment)
                .AddAmazonS3Client(Configuration.GetSection("AmazonS3Configuration")["AccessKey"], Configuration.GetSection("AmazonS3Configuration")["SecretKey"])
                .AddGateways()
                .AddIOptionsConfiguration(Configuration)
                .ConfigureAddressProviders()
                .ConfigureOrganisationProviders()
                .ConfigureStreetProviders()
                .ConfigurePaymentProviders()
                .ConfigureDocumentCreationProviders()
                .AddHelpers()
                .AddServices()
                .AddWorkflows()
                .AddFactories()
                .AddSession(_ => {
                    _.IdleTimeout = TimeSpan.FromMinutes(30);
                    _.Cookie.Path = "/";
                });
                
            services.AddTransient<ICache, Cache.Cache>();
            services.Configure<SubmissionServiceConfiguration>(Configuration.GetSection("SubmissionServiceConfiguration"));
            services.AddTransient<ITagManagerConfiguration, TagManagerConfiguration>();
            services.AddMvc()
                .AddMvcOptions(options => {
                    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
                    options.ModelBinderProviders.Insert(0, new CustomFormFileModelBinderProvider());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddResilientHttpClients<IGateway, Gateway>(Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
