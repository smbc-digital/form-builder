using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using form_builder.Extensions;
using form_builder.Helpers;
using StockportGovUK.AspNetCore.Gateways;

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
                .AddCacheProvider(Configuration.GetSection("RedisConfiguration")["RedisUrl"])
                .AddValidators()
                .AddSchemaProvider(HostingEnvironment)
                .AddAmazonS3Client(Configuration.GetSection("AmazonS3Configuration")["AccessKey"], Configuration.GetSection("AmazonS3Configuration")["SecretKey"])
                .AddGateways()
                .AddIOptionsConfiguration(Configuration)
                .AddHelpers();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddScoped<IViewRender, ViewRender>();
            services.AddResilientHttpClients<IGateway, StockportGovUK.AspNetCore.Gateways.Gateway>(Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsEnvironment("local") || env.IsEnvironment("uitest"))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

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
