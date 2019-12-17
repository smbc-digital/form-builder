using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using form_builder.Extensions;
using StockportGovUK.AspNetCore.Gateways;
using form_builder.Configuration;
using StockportGovUK.AspNetCore.Middleware.App;

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
            services
                .ConfigureCookiePolicy()
                .AddValidators()
                .AddStorageProvider(Configuration)
                .AddSchemaProvider(HostingEnvironment)
                .AddAmazonS3Client(Configuration.GetSection("AmazonS3Configuration")["AccessKey"], Configuration.GetSection("AmazonS3Configuration")["SecretKey"])
                .AddGateways()
                .AddIOptionsConfiguration(Configuration)
                .ConfigureAddressProviders()
                .ConfigureStreetProviders()
                .AddHelpers()
                .AddServices()
                .AddSession(_ => _.IdleTimeout = TimeSpan.FromMinutes(30));

            services.AddTransient<ITagManagerConfiguration, TagManagerConfiguration>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
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

            app.UseSession();

            app.UseHttpsRedirection();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            app.UseStaticFiles();
        }
    }
}
