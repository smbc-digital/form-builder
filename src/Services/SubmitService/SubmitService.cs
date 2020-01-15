using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Services.SubmitService.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Gateways.ComplimentsComplaintsServiceGateway;
using System.Dynamic;
using System.Linq;
using form_builder.Models.Elements;
using form_builder.Mappers;

namespace form_builder.Services.SubmtiService
{
    public interface ISubmitService
    {
        Task<SubmitServiceEntity> ProcessSubmission(string form);
    }

    public class SubmitService : ISubmitService
    {
        private readonly IDistributedCacheWrapper _distributedCache;

        private readonly ISchemaProvider _schemaProvider;

        private readonly IGateway _gateway;

        private readonly IComplimentsComplaintsServiceGateway _complimentsComplaintsServiceGateway;

        private readonly IPageHelper _pageHelper;

        private readonly ISessionHelper _sessionHelper;

        private readonly ILogger<SubmitService> _logger;

        public SubmitService(ILogger<SubmitService> logger, IDistributedCacheWrapper distributedCache, ISchemaProvider schemaProvider, IGateway gateway, IComplimentsComplaintsServiceGateway complimentsComplaintsServiceGateway, IPageHelper pageHelper, ISessionHelper sessionHelper)
        {
            _distributedCache = distributedCache;
            _schemaProvider = schemaProvider;
            _gateway = gateway;
            _complimentsComplaintsServiceGateway = complimentsComplaintsServiceGateway;
            _pageHelper = pageHelper;
            _sessionHelper = sessionHelper;
            _logger = logger;
        }

        public async Task<SubmitServiceEntity> ProcessSubmission(string form)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
            {
                throw new ApplicationException($"A Session GUID was not provided.");
            }

            var baseForm = await _schemaProvider.Get<FormSchema>(form);
            var formData = _distributedCache.GetString(sessionGuid);
            var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);
            convertedAnswers.FormName = form;

            var currentPage = baseForm.GetPage(convertedAnswers.Path);
            var postUrl = currentPage.GetSubmitFormEndpoint(convertedAnswers);
            var postData = CreatePostData(convertedAnswers, baseForm);
            var reference = string.Empty;

            var response = await _gateway.PostAsync(postUrl, postData);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException($"SubmitService::ProcessSubmission, An exception has occured while attemping to call {postUrl}, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");
            }

            if (response.Content != null)
            {
                var content = await response.Content.ReadAsStringAsync() ?? string.Empty;
                reference = JsonConvert.DeserializeObject<string>(content);
            }

            _distributedCache.Remove(sessionGuid);
            _sessionHelper.RemoveSessionGuid();

            var page = baseForm.GetPage("success");

            if (page == null)
            {
                return new SubmitServiceEntity
                {
                    ViewName = "Submit",
                    ViewModel = convertedAnswers,
                    FeedbackFormUrl = baseForm.FeedbackForm
                };
            }

            var viewModel = await _pageHelper.GenerateHtml(page, new Dictionary<string, string>(), baseForm, sessionGuid);
            var success = new Success
            {
                FormName = baseForm.FormName,
                Reference = reference,
                FormAnswers = convertedAnswers,
                PageContent = viewModel.RawHTML,
                SecondaryHeader = page.Title
            };

            return new SubmitServiceEntity
            {
                ViewName = "Success",
                ViewModel = success,
                FeedbackFormUrl = baseForm.FeedbackForm
            };
        }

        private object CreatePostData(FormAnswers formAnswers, FormSchema formSchema)
        {
            var data = new ExpandoObject() as IDictionary<string, object>;

            var keys = formSchema.Pages.SelectMany(_ => _.ValidatableElements)
               .Select(_ => new
               {
                   TargetMapping = string.IsNullOrEmpty(_.Properties.TargetMapping) ? _.Properties.QuestionId : _.Properties.TargetMapping,
                   Element = _
               })
               .ToList();

            keys.ForEach(_ =>
            {
                data = RecursiveCheckAndCreate(_.TargetMapping, _.Element, formAnswers, data);
            });

            return data;
        }

        private IDictionary<string, object> RecursiveCheckAndCreate(string targetMapping, IElement element, FormAnswers formAnswers, IDictionary<string, object> obj)
        {
            var splitTargets = targetMapping.Split(".");

            if (splitTargets.Length == 1)
            {
                object objectValue;
                if (obj.TryGetValue(splitTargets[0], out objectValue))
                {
                    var combinedValue = $"{objectValue} {ElementMapper.GetAnswerValue(element, formAnswers)}";
                    obj.Remove(splitTargets[0]);
                    obj.Add(splitTargets[0], combinedValue);
                    return obj;
                }

                obj.Add(splitTargets[0], ElementMapper.GetAnswerValue(element, formAnswers));
                return obj;
            }

            object subObject;
            if (!obj.TryGetValue(splitTargets[0], out subObject))
                subObject = new ExpandoObject();

            subObject = RecursiveCheckAndCreate(targetMapping.Replace($"{splitTargets[0]}.", ""), element, formAnswers, subObject as IDictionary<string, object>);

            obj.Remove(splitTargets[0]);
            obj.Add(splitTargets[0], subObject);

            return obj;
        }
    }
}