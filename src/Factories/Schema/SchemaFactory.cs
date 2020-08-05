using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Factories.Transform.Lookups;
using form_builder.Factories.Transform.ReusableElements;
using form_builder.Models;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Factories.Schema
{
    public interface ISchemaFactory
    {
        Task<FormSchema> Build(string formKey);
    }

    public class SchemaFactory : ISchemaFactory
    {
        private readonly IDistributedCacheWrapper _distrbutedCache;
        private readonly ILookupSchemaTransformFactory _lookupSchemaFactory;
        private readonly IReusableElementSchemaTransformFactory _reusableElementSchemaFactory;
        private readonly ISchemaProvider _schemaProvider;
        private readonly DistrbutedCacheConfiguration _distrbutedCacheConfiguration;
        private readonly DistributedCacheExpirationConfiguration _distrbutedCacheExpirationConfiguration;

        public SchemaFactory(IDistributedCacheWrapper distrbutedCache, ISchemaProvider schemaProvider, ILookupSchemaTransformFactory lookupSchemaFactory, IReusableElementSchemaTransformFactory reusableElementSchemaFactory, IOptions<DistrbutedCacheConfiguration> distrbutedCacheConfiguration, IOptions<DistributedCacheExpirationConfiguration> distrbutedCacheExpirationConfiguration)
        {
            _distrbutedCache = distrbutedCache;
            _schemaProvider = schemaProvider;
            _lookupSchemaFactory = lookupSchemaFactory;
            _reusableElementSchemaFactory = reusableElementSchemaFactory;
            _distrbutedCacheConfiguration = distrbutedCacheConfiguration.Value;
            _distrbutedCacheExpirationConfiguration = distrbutedCacheExpirationConfiguration.Value;
        }

        public async Task<FormSchema> Build(string formKey)
        {
            if(_distrbutedCacheConfiguration.UseDistrbutedCache && _distrbutedCacheExpirationConfiguration.FormJson > 0)
            {
                var data = _distrbutedCache.GetString($"{ESchemaType.FormJson.ToESchemaTypePrefix()}{formKey}");

                if(data != null)
                    return JsonConvert.DeserializeObject<FormSchema>(data);
            }

            var formSchema = await _schemaProvider.Get<FormSchema>(formKey);
            
            formSchema = await _reusableElementSchemaFactory.Transform(formSchema);
            formSchema = await _lookupSchemaFactory.Transform(formSchema);

            if(_distrbutedCacheConfiguration.UseDistrbutedCache && _distrbutedCacheExpirationConfiguration.FormJson > 0)
            {
                await _distrbutedCache.SetStringAsync($"{ESchemaType.FormJson.ToESchemaTypePrefix()}{formKey}", JsonConvert.SerializeObject(formSchema), _distrbutedCacheExpirationConfiguration.FormJson);
            }

            return formSchema;
        }
    }
}
