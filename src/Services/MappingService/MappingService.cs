using form_builder.Cache;
using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Mappers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using form_builder.Services.MappingService.Entities;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Services.MappingService
{
    public interface IMappingService
    {
        Task<MappingEntity> Map(string sessionGuid, string form);
    }

    public class MappingService : IMappingService
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IElementMapper _elementMapper;
        private readonly ICache _cache;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;

        public MappingService(IDistributedCacheWrapper distributedCache,
            IElementMapper elementMapper, ICache cache, IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration)
        {
            _distributedCache = distributedCache;
            _elementMapper = elementMapper;
            _cache = cache;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
        }

        public async Task<MappingEntity> Map(string sessionGuid, string form)
        {
            var baseForm = await _cache.GetFromCacheOrDirectlyFromSchemaAsync<FormSchema>(form, _distributedCacheExpirationConfiguration.FormJson, ESchemaType.FormJson);

            var formData = _distributedCache.GetString(sessionGuid);
            var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);
            convertedAnswers.FormName = form;

            return new MappingEntity
            {
                Data = CreatePostData(convertedAnswers, baseForm),
                BaseForm = baseForm,
                FormAnswers = convertedAnswers
            };
        }

        private object CreatePostData(FormAnswers formAnswers, FormSchema formSchema)
        {
            var data = new ExpandoObject() as IDictionary<string, dynamic>;

            formSchema.Pages.SelectMany(_ => _.ValidatableElements)
                .ToList()
                .ForEach(_ => data = RecursiveCheckAndCreate(string.IsNullOrEmpty(_.Properties.TargetMapping) ? _.Properties.QuestionId : _.Properties.TargetMapping, _, formAnswers, data));

            return data;
        }

        private IDictionary<string, dynamic> RecursiveCheckAndCreate(string targetMapping, IElement element, FormAnswers formAnswers, IDictionary<string, dynamic> obj)
        {
            var splitTargets = targetMapping.Split(".");

            if (splitTargets.Length == 1)
            {
                object objectValue;
                if (obj.TryGetValue(splitTargets[0], out objectValue))
                {
                    var combinedValue = $"{objectValue} {_elementMapper.GetAnswerValue(element, formAnswers)}";
                    obj.Remove(splitTargets[0]);
                    obj.Add(splitTargets[0], combinedValue);
                    return obj;
                }

                obj.Add(splitTargets[0], _elementMapper.GetAnswerValue(element, formAnswers));
                return obj;
            }

            object subObject;
            if (!obj.TryGetValue(splitTargets[0], out subObject))
                subObject = new ExpandoObject();

            subObject = RecursiveCheckAndCreate(targetMapping.Replace($"{splitTargets[0]}.", ""), element, formAnswers, subObject as IDictionary<string, dynamic>);

            obj.Remove(splitTargets[0]);
            obj.Add(splitTargets[0], subObject);

            return obj;
        }
    }
}
