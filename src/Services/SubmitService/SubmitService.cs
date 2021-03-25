using System;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Extensions;
using form_builder.Factories.Schema;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.ReferenceNumbers;
using form_builder.Providers.StorageProvider;
using form_builder.Services.MappingService.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;

namespace form_builder.Services.SubmitService
{
    public class SubmitService : ISubmitService
    {
        private readonly IGateway _gateway;

        private readonly IPageHelper _pageHelper;

        private readonly IWebHostEnvironment _environment;

        private readonly SubmissionServiceConfiguration _submissionServiceConfiguration;

        private readonly IDistributedCacheWrapper _distributedCache;

        private readonly ISchemaFactory _schemaFactory;

        private readonly IReferenceNumberProvider _referenceNumberProvider;

        public SubmitService(
            IGateway gateway,
            IPageHelper pageHelper,
            IWebHostEnvironment environment,
            IOptions<SubmissionServiceConfiguration> submissionServiceConfiguration,
            IDistributedCacheWrapper distributedCache,  
            ISchemaFactory schemaFactory,
            IReferenceNumberProvider referenceNumberProvider)
        {
            _gateway = gateway;
            _pageHelper = pageHelper;
            _environment = environment;
            _submissionServiceConfiguration = submissionServiceConfiguration.Value;
            _distributedCache = distributedCache;
            _schemaFactory = schemaFactory;
            _referenceNumberProvider = referenceNumberProvider;
        }

        public async Task PreProcessSubmission(string form, string sessionGuid)
        {
            var baseForm = await _schemaFactory.Build(form);
            if (baseForm.GenerateReferenceNumber)
                _pageHelper.SaveCaseReference(sessionGuid, _referenceNumberProvider.GetReference(baseForm.ReferencePrefix), true, baseForm.GeneratedReferenceNumberMapping);
        }

        public async Task<string> ProcessSubmission(MappingEntity mappingEntity, string form, string sessionGuid)
        {
            var baseForm = await _schemaFactory.Build(form);
            var reference = string.Empty;

            if (baseForm.GenerateReferenceNumber)
            {
                var answers = JsonConvert.DeserializeObject<FormAnswers>(_distributedCache.GetString(sessionGuid));
                reference = answers.CaseReference;
            }

            return _submissionServiceConfiguration.FakeSubmission 
                ? ProcessFakeSubmission(sessionGuid, reference) 
                : await ProcessGenuineSubmission(mappingEntity, form, sessionGuid, baseForm, reference);
        }

        private string ProcessFakeSubmission(string sessionGuid, string reference)
        {
                if (!string.IsNullOrEmpty(reference))
                    return reference;
                    
                _pageHelper.SaveCaseReference(sessionGuid, "123456");
                return "123456";
        }

        private async Task<string> ProcessGenuineSubmission(MappingEntity mappingEntity, string form, string sessionGuid, FormSchema baseForm, string reference)
        {
            var currentPage = mappingEntity.BaseForm.GetPage(_pageHelper, mappingEntity.FormAnswers.Path);
            var submitSlug = currentPage.GetSubmitFormEndpoint(mappingEntity.FormAnswers, _environment.EnvironmentName.ToS3EnvPrefix());

            System.Net.Http.HttpResponseMessage response;

            var json = JsonConvert.SerializeObject(mappingEntity.Data);

            response = await _gateway.PostAsync(mappingEntity, submitSlug);

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"SubmitService::ProcessSubmission, An exception has occurred while attempting to call {submitSlug.URL}, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");
            

            if (!baseForm.GenerateReferenceNumber && response.Content != null)
            {
                var content = await response.Content.ReadAsStringAsync() ?? string.Empty;
                reference = JsonConvert.DeserializeObject<string>(content);
                _pageHelper.SaveCaseReference(sessionGuid, reference);
            }

            return reference;
        }

        public async Task<string> PaymentSubmission(MappingEntity mappingEntity, string form, string sessionGuid)
        {
            if (_submissionServiceConfiguration.FakePaymentSubmission)
            {
                return "123456";
            }

            var currentPage = mappingEntity.BaseForm.GetPage(_pageHelper, mappingEntity.FormAnswers.Path);

            var postUrl = currentPage.GetSubmitFormEndpoint(mappingEntity.FormAnswers, _environment.EnvironmentName.ToS3EnvPrefix());

            if (string.IsNullOrEmpty(postUrl.URL))
            {
                throw new ApplicationException($"SubmitService::PaymentSubmission, No submission URL has been provided for FORM: { form }, ENVIRONMENT: { _environment.EnvironmentName }");
            }

            if (string.IsNullOrWhiteSpace(postUrl.AuthToken))
            {
                _gateway.ChangeAuthenticationHeader(string.Empty);
            }
            else
            {
                _gateway.ChangeAuthenticationHeader(postUrl.AuthToken);
            }

            var response = await _gateway.PostAsync(postUrl.URL, mappingEntity.Data);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException($"SubmitService::PaymentSubmission, An exception has occured while attempting to call {postUrl.URL}, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");
            }

            if (response.Content != null)
            {
                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                {
                    throw new ApplicationException($"SubmitService::PaymentSubmission, Gateway {postUrl.URL} responded with empty reference");
                }

                return JsonConvert.DeserializeObject<string>(content);
            }

            throw new ApplicationException($"SubmitService::PaymentSubmission, An exception has occured when response content from {postUrl} is null, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");
        }
    }
}
