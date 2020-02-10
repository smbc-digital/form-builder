using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
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
using form_builder.Services.MappingService.Entities;
using Microsoft.AspNetCore.Hosting;
using form_builder.Extensions;
using System.Linq;

namespace form_builder.Services.SubmtiService
{
    public interface ISubmitService
    {
        Task<SubmitServiceEntity> ProcessSubmission(MappingEntity mappingEntity, string form, string sessionGuid);
        Task<string> PaymentSubmission(MappingEntity mappingEntity, string form, string sessionGuid);
    }
    public class SubmitService : ISubmitService
    {
        private readonly IDistributedCacheWrapper _distributedCache;

        private readonly IGateway _gateway;

        private readonly IComplimentsComplaintsServiceGateway _complimentsComplaintsServiceGateway;

        private readonly IPageHelper _pageHelper;

        private readonly ISessionHelper _sessionHelper;

        private readonly ILogger<SubmitService> _logger;

        private readonly IHostingEnvironment _environment;



        public SubmitService(ILogger<SubmitService> logger, IDistributedCacheWrapper distributedCache, IGateway gateway, IComplimentsComplaintsServiceGateway complimentsComplaintsServiceGateway, IPageHelper pageHelper, ISessionHelper sessionHelper, IHostingEnvironment environment)
        {
            _distributedCache = distributedCache;
            _gateway = gateway;
            _complimentsComplaintsServiceGateway = complimentsComplaintsServiceGateway;
            _pageHelper = pageHelper;
            _sessionHelper = sessionHelper;
            _logger = logger;
            _environment = environment;
        }

        public async Task<SubmitServiceEntity> ProcessSubmission(MappingEntity mappingEntity, string form, string sessionGuid)
        {
            var reference = string.Empty;

            var currentPage = mappingEntity.BaseForm.GetPage(mappingEntity.FormAnswers.Path);
            var submitSlug = currentPage.GetSubmitFormEndpoint(mappingEntity.FormAnswers, _environment.EnvironmentName.ToS3EnvPrefix());
            if (string.IsNullOrEmpty(submitSlug.AuthToken) || string.IsNullOrEmpty(submitSlug.URL))
            {
                throw new ApplicationException($"The AuthToken or URL is empty for this form: { form }");
            }

            if (form == "give-a-compliment" || form == "give-feedback" || form == "make-a-formal-complaint")
            {
                var response = await _complimentsComplaintsServiceGateway.SubmitForm(submitSlug.URL, mappingEntity.Data);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new ApplicationException($"SubmitService::ProcessSubmission, An exception has occured while attemping to call {submitSlug.URL}, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");
                }

                if (response.ResponseContent != null)
                {
                    reference = JsonConvert.DeserializeObject<string>(response.ResponseContent);
                }
            }
            else
            {
                _gateway.ChangeAuthenticationHeader(submitSlug.AuthToken);
                var response = await _gateway.PostAsync(submitSlug.URL, mappingEntity.Data);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new ApplicationException($"SubmitService::ProcessSubmission, An exception has occured while attemping to call {submitSlug.URL}, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");
                }

                if (response.Content != null)
                {
                    var content = await response.Content.ReadAsStringAsync() ?? string.Empty;
                    reference = JsonConvert.DeserializeObject<string>(content);
                }
            }

            _distributedCache.Remove(sessionGuid);
            _sessionHelper.RemoveSessionGuid();

            var page = mappingEntity.BaseForm.GetPage("success");

            if (page == null)
            {
                return new SubmitServiceEntity
                {
                    ViewName = "Submit",
                    ViewModel = mappingEntity.FormAnswers,
                    FeedbackFormUrl = mappingEntity.BaseForm.FeedbackForm
                };
            }

            var viewModel = await _pageHelper.GenerateHtml(page, new Dictionary<string, string>(), mappingEntity.BaseForm, sessionGuid);
            var success = new Success
            {
                FormName = mappingEntity.BaseForm.FormName,
                Reference = reference,
                FormAnswers = mappingEntity.FormAnswers,
                PageContent = viewModel.RawHTML,
                SecondaryHeader = page.Title
            };

            return new SubmitServiceEntity
            {
                ViewName = "Success",
                ViewModel = success,
                FeedbackFormUrl = mappingEntity.BaseForm.FeedbackForm
            };
        }

        public async Task<string> PaymentSubmission(MappingEntity mappingEntity, string form, string sessionGuid)
        {
            var reference = string.Empty;

            var currentPage = mappingEntity.BaseForm.GetPage(mappingEntity.FormAnswers.Path);

            var postUrl = currentPage.GetSubmitFormEndpoint(mappingEntity.FormAnswers, _environment.EnvironmentName.ToS3EnvPrefix());

            var response = await _gateway.PostAsync(postUrl.URL, mappingEntity.Data);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException($"SubmitService::PaymentSubmission, An exception has occured while attemping to call {postUrl}, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");
            }

            if (response.Content != null)
            {
                var content = await response.Content.ReadAsStringAsync();

                if(string.IsNullOrWhiteSpace(content)){
                    throw new ApplicationException($"SubmitService::PaymentSubmission, Gateway {postUrl} responsed with empty reference");
                }
                
                return JsonConvert.DeserializeObject<string>(content);
            }

            throw new ApplicationException($"SubmitService::PaymentSubmission, An exception has occured when response content from {postUrl} is null, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");
        }
    }
}