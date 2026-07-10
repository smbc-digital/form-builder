using IPNetwork = Microsoft.AspNetCore.HttpOverrides.IPNetwork;

var builder = WebApplication.CreateBuilder(args);

CultureInfo.CurrentCulture = new CultureInfo("en-GB");
CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-GB");
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.All;
    options.KnownNetworks.Add(new IPNetwork(System.Net.IPAddress.Parse("10.0.0.0"), 8));
});

builder.Services
    .AddRazorViewEngineViewLocations()
    .ConfigureCookiePolicy()
    .AddValidators()
    .AddTagParsers()
    .AddReferenceNumberProvider()
    .AddStorageProvider(builder.Configuration)
    .AddFileStorageProvider(builder.Configuration)
    .AddSchemaProvider(builder.Environment)
    .AddTransformDataProvider(builder.Environment)
    .AddAmazonS3Client(builder.Configuration.GetSection("AmazonS3Configuration")["AccessKey"], builder.Configuration.GetSection("AmazonS3Configuration")["SecretKey"])
    .AddSesEmailConfiguration(builder.Configuration.GetSection("Ses")["Accesskey"], builder.Configuration.GetSection("Ses")["Secretkey"])
    .AddGateways(builder.Configuration)
    .AddIOptionsConfiguration(builder.Configuration)
    .AddUtilities()
    .AddAnalyticsProviders()
    .ConfigureAddressProviders()
    .ConfigureCorsPolicy()
    .ConfigureDynamicLookDataProviders()
    .ConfigureOrganisationProviders()
    .ConfigureStreetProviders()
    .ConfigureSubmitProviders()
    .ConfigurePaymentProviders(builder.Environment)
    .ConfigureBookingProviders()
    .ConfigureEmailTemplateProviders()
    .ConfigureDocumentCreationProviders()
    .ConfigureFormAnswersProviders()
    .ConfigureFormatters()
    .ConfigureEnabledFor()
    .ConfigureEmailProviders(builder.Environment)
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
        _.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        _.Cookie.SameSite = SameSiteMode.None;
    })
    .AddSession(_ =>
    {
        _.IdleTimeout = TimeSpan.FromMinutes(60);
        _.Cookie.Path = "/";
        _.Cookie.Name = ".formbuilder.v2";
        _.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        _.Cookie.SameSite = SameSiteMode.None;
    });

builder.Services
    .AddMvc()
    .AddMvcOptions(options =>
    {
        options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
        options.ModelBinderProviders.Insert(0, new CustomFormFileModelBinderProvider());
    });

builder.Services
    .AddFeatureManagement();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteToElasticsearchAws(builder.Configuration)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Services.AddSerilog(Log.Logger);

var app = builder.Build();
app.UseForwardedHeaders();

if (builder.Environment.IsEnvironment("local") || builder.Environment.IsEnvironment("uitest"))
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
    .UseMiddleware<CookiesComplianceMiddleware>()
    .UseStaticFiles()
    .UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(
            Path.Combine(builder.Environment.ContentRootPath, "fonts")),
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

app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }
