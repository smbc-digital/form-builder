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
using StockportGovUK.NetStandard.Models.FileManagement;
using form_builder.Factories.Schema;
using Amazon.S3.Model;
using Microsoft.IdentityModel.Tokens;
using form_builder.Extensions;

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
        private readonly ISchemaFactory _schemaFactory;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;

        public MappingService(IDistributedCacheWrapper distributedCache,
            IElementMapper elementMapper, 
            ISchemaFactory schemaFactory,
            IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration)
        {
            _distributedCache = distributedCache;
            _elementMapper = elementMapper;
            _schemaFactory = schemaFactory;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
        }

        public async Task<MappingEntity> Map(string sessionGuid, string form)
        {
            var baseForm = await _schemaFactory.Build(form);

            var formData = _distributedCache.GetString(sessionGuid);
            var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            convertedAnswers.FormName = form;
            convertedAnswers.Pages = convertedAnswers.GetReducedAnswers(baseForm);

            return new MappingEntity
            {
                Data = CreatePostData(convertedAnswers, baseForm),
                BaseForm = baseForm,
                FormAnswers = convertedAnswers,
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
                if (element.Type == EElementType.FileUpload)
                    return CheckAndCreateForFileUpload(splitTargets[0], element, formAnswers, obj);

                object answerValue = _elementMapper.GetAnswerValue(element, formAnswers);
                if (answerValue != null && obj.TryGetValue(splitTargets[0], out var objectValue))
                {
                    var combinedValue = $"{objectValue} {answerValue}";
                    obj.Remove(splitTargets[0]);
                    obj.Add(splitTargets[0], combinedValue.Trim());
                    return obj;
                }

                if(answerValue != null)
                    obj.Add(splitTargets[0], answerValue);

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

        private IDictionary<string, dynamic> CheckAndCreateForFileUpload(string target, IElement element, FormAnswers formAnswers, IDictionary<string, dynamic> obj)
        {
            object objectValue;
            var value = _elementMapper.GetAnswerValue(element, formAnswers);

            if (obj.TryGetValue(target, out objectValue))
            {
                var files = (List<File>) objectValue;

                if (value != null)
                {
                    obj.Remove(target);
                    files.Add((File) value);
                    obj.Add(target, files);
                }
                return obj;
            }
            else
            {
                var files = new List<File>();
                if (value != null)
                {
                    files.Add((File) value);
                    obj.Add(target, files);
                }
            }

            return obj;
        }
    }
}
