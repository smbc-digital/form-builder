using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Factories.Transform.Lookups;
using form_builder.Factories.Transform.ReusableElements;
using form_builder.Models;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Factories.Schema
{
    public class SchemaFactory : ISchemaFactory
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly ILookupSchemaTransformFactory _lookupSchemaFactory;
        private readonly IReusableElementSchemaTransformFactory _reusableElementSchemaFactory;
        private readonly ISchemaProvider _schemaProvider;
        private readonly DistributedCacheConfiguration _distributedCacheConfiguration;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;
        private readonly IConfiguration _configuration;
        private readonly IFormSchemaIntegrityValidator _formSchemaIntegrityValidator;

        public SchemaFactory(IDistributedCacheWrapper distributedCache,
            ISchemaProvider schemaProvider,
            ILookupSchemaTransformFactory lookupSchemaFactory,
            IReusableElementSchemaTransformFactory reusableElementSchemaFactory,
            IOptions<DistributedCacheConfiguration> distributedCacheConfiguration,
            IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration,
            IConfiguration configuration,
            IFormSchemaIntegrityValidator formSchemaIntegrityValidator)
        {
            _distributedCache = distributedCache;
            _schemaProvider = schemaProvider;
            _lookupSchemaFactory = lookupSchemaFactory;
            _reusableElementSchemaFactory = reusableElementSchemaFactory;
            _distributedCacheConfiguration = distributedCacheConfiguration.Value;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
            _configuration = configuration;
            _formSchemaIntegrityValidator = formSchemaIntegrityValidator;
        }

        public async Task<FormSchema> Build(string formKey)
        {
            if (!_schemaProvider.ValidateSchemaName(formKey).Result)
                return null;

            if (_distributedCacheConfiguration.UseDistributedCache && _distributedCacheExpirationConfiguration.FormJson > 0)
            {
                string data = _distributedCache.GetString($"{ESchemaType.FormJson.ToESchemaTypePrefix(_configuration["ApplicationVersion"])}{formKey}");

                if (data != null)
                    return JsonConvert.DeserializeObject<FormSchema>(data);
            }

            FormSchema formSchema = await _schemaProvider.Get<FormSchema>(formKey);
            formSchema = await _reusableElementSchemaFactory.Transform(formSchema);
            formSchema = _lookupSchemaFactory.Transform(formSchema);

            await _formSchemaIntegrityValidator.Validate(formSchema);

            if (_distributedCacheConfiguration.UseDistributedCache && _distributedCacheExpirationConfiguration.FormJson > 0)
                await _distributedCache.SetStringAsync($"{ESchemaType.FormJson.ToESchemaTypePrefix(_configuration["ApplicationVersion"])}{formKey}", JsonConvert.SerializeObject(formSchema), _distributedCacheExpirationConfiguration.FormJson);

            return formSchema;
        }
    }
}