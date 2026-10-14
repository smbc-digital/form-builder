namespace form_builder.Factories.Schema;

public class SchemaFactory(IDistributedCacheWrapper distributedCache,
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
    private readonly DistributedCacheConfiguration _distributedCacheConfiguration = distributedCacheConfiguration.Value;
    private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;

    public async Task<FormSchema> Build(string formKey)
    {
        if (previewModeConfiguration.Value.IsEnabled && formKey.StartsWith(PreviewConstants.PREVIEW_MODE_PREFIX))
            return await InPreviewMode(formKey);

        if (!schemaProvider.ValidateSchemaName(formKey).Result)
            return null;

        if (_distributedCacheConfiguration.UseDistributedCache && _distributedCacheExpirationConfiguration.FormJson > 0)
        {
            string data = distributedCache.GetString($"{ESchemaType.FormJson.ToESchemaTypePrefix()}{formKey}");
            if (data is not null)
            {
                return JsonConvert.DeserializeObject<FormSchema>(data);
            }
        }

        FormSchema formSchema = await schemaProvider.Get<FormSchema>(formKey);

        formSchema = await reusableElementSchemaFactory.Transform(formSchema);
        formSchema = lookupSchemaFactory.Transform(formSchema);

        await formSchemaIntegrityValidator.Validate(formSchema);

        if (_distributedCacheConfiguration.UseDistributedCache && _distributedCacheExpirationConfiguration.FormJson > 0)
            await distributedCache.SetStringAbsoluteAsync($"{ESchemaType.FormJson.ToESchemaTypePrefix()}{formKey}", JsonConvert.SerializeObject(formSchema), _distributedCacheExpirationConfiguration.FormJson);

        return formSchema;
    }

    private async Task<FormSchema> InPreviewMode(string formKey)
    {
        var data = distributedCache.GetString($"{ESchemaType.FormJson.ToESchemaTypePrefix()}{formKey}");

        if (data is null)
            throw new ApplicationException("SchemaFactory::InPreviewMode, Requested form has expired");

        FormSchema formSchema = JsonConvert.DeserializeObject<FormSchema>(data);
        formSchema = await reusableElementSchemaFactory.Transform(formSchema);
        formSchema = lookupSchemaFactory.Transform(formSchema);

        await formSchemaIntegrityValidator.Validate(formSchema);

        return formSchema;
    }

    public async Task<Page> TransformPage(Page page, FormAnswers convertedAnswers)
    {
        foreach (var userPageFactory in userPageTransformFactories)
            await userPageFactory.Transform(page, convertedAnswers);

        return page;
    }
}