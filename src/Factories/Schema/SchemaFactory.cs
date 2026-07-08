using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Factories.Transform.Lookups;
using form_builder.Factories.Transform.ReusableElements;
using form_builder.Factories.Transform.UserSchema;
using form_builder.Models;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Validators.IntegrityChecks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Factories.Schema;

public class SchemaFactory(
    IDistributedCacheWrapper distributedCache,
    ISchemaProvider schemaProvider,
    ILookupSchemaTransformFactory lookupSchemaFactory,
    IReusableElementSchemaTransformFactory reusableElementSchemaFactory,
    IOptions<DistributedCacheConfiguration> distributedCacheConfiguration,
    IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration,
    IOptions<PreviewModeConfiguration> previewModeConfiguration,
    IFormSchemaIntegrityValidator formSchemaIntegrityValidator,
    IEnumerable<IUserPageTransformFactory> userPageTransformFactories)
    : ISchemaFactory
{
    private readonly IDistributedCacheWrapper _distributedCache = distributedCache;
    private readonly ILookupSchemaTransformFactory _lookupSchemaFactory = lookupSchemaFactory;
    private readonly IReusableElementSchemaTransformFactory _reusableElementSchemaFactory = reusableElementSchemaFactory;
    private readonly ISchemaProvider _schemaProvider = schemaProvider;
    private readonly DistributedCacheConfiguration _distributedCacheConfiguration = distributedCacheConfiguration.Value;
    private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
    private readonly IOptions<PreviewModeConfiguration> _previewModeConfiguration = previewModeConfiguration;
    private readonly IFormSchemaIntegrityValidator _formSchemaIntegrityValidator = formSchemaIntegrityValidator;
    private readonly IEnumerable<IUserPageTransformFactory> _userPageTransformFactories = userPageTransformFactories;

    public async Task<FormSchema> Build(string formKey)
    {
        if (_previewModeConfiguration.Value.IsEnabled && formKey.StartsWith(PreviewConstants.PREVIEW_MODE_PREFIX))
            return await InPreviewMode(formKey);

        if (!_schemaProvider.ValidateSchemaName(formKey).Result)
            return null;

        if (_distributedCacheConfiguration.UseDistributedCache && _distributedCacheExpirationConfiguration.FormJson > 0)
        {
            string data = _distributedCache.GetString($"{ESchemaType.FormJson.ToESchemaTypePrefix()}{formKey}");
            if (data is not null)
            {
                return JsonConvert.DeserializeObject<FormSchema>(data);
            }
        }

        FormSchema formSchema = await _schemaProvider.Get<FormSchema>(formKey);

        formSchema = await _reusableElementSchemaFactory.Transform(formSchema);
        formSchema = _lookupSchemaFactory.Transform(formSchema);

        await _formSchemaIntegrityValidator.Validate(formSchema);

        if (_distributedCacheConfiguration.UseDistributedCache && _distributedCacheExpirationConfiguration.FormJson > 0)
            await _distributedCache.SetStringAbsoluteAsync($"{ESchemaType.FormJson.ToESchemaTypePrefix()}{formKey}", JsonConvert.SerializeObject(formSchema), _distributedCacheExpirationConfiguration.FormJson);

        return formSchema;
    }

    private async Task<FormSchema> InPreviewMode(string formKey)
    {
        var data = _distributedCache.GetString($"{ESchemaType.FormJson.ToESchemaTypePrefix()}{formKey}");

        if (data is null)
            throw new ApplicationException("ScheamFactory::InPreviewMode, Requested form has expired");

        FormSchema formSchema = JsonConvert.DeserializeObject<FormSchema>(data);
        formSchema = await _reusableElementSchemaFactory.Transform(formSchema);
        formSchema = _lookupSchemaFactory.Transform(formSchema);

        await _formSchemaIntegrityValidator.Validate(formSchema);

        return formSchema;
    }

    public async Task<Page> TransformPage(Page page, FormAnswers convertedAnswers)
    {
        foreach (var userPageFactory in _userPageTransformFactories)
            await userPageFactory.Transform(page, convertedAnswers);

        return page;
    }
}