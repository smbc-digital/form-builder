using System;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Extensions;
using form_builder.Helpers.PageHelpers;
using form_builder.Services.MappingService.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;

namespace form_builder.Services.SubmitService
{
    public interface ISubmitService
    {
        Task<string> ProcessSubmission(MappingEntity mappingEntity, string form, string sessionGuid);
        Task<string> PaymentSubmission(MappingEntity mappingEntity, string form, string sessionGuid);

    }
    public class SubmitService : ISubmitService
    {
        private readonly IGateway _gateway;

        private readonly IPageHelper _pageHelper;

        private readonly IWebHostEnvironment _environment;

        private readonly SubmissionServiceConfiguration _submissionServiceConfiguration;


        public SubmitService(
            IGateway gateway, 
            IPageHelper pageHelper, 
            IWebHostEnvironment environment, 
            IOptions<SubmissionServiceConfiguration> submissionServiceConfiguration)
        {
            _gateway = gateway;
            _pageHelper = pageHelper;
            _environment = environment;
            _submissionServiceConfiguration = submissionServiceConfiguration.Value;
        }

        public async Task<string> ProcessSubmission(MappingEntity mappingEntity, string form, string sessionGuid)
        {
            if(_submissionServiceConfiguration.FakeSubmission)
            {
                return "123456"; 
            }
            var reference = string.Empty;

            var currentPage = mappingEntity.BaseForm.GetPage(_pageHelper, mappingEntity.FormAnswers.Path);
            var submitSlug = currentPage.GetSubmitFormEndpoint(mappingEntity.FormAnswers, _environment.EnvironmentName.ToS3EnvPrefix());

            _gateway.ChangeAuthenticationHeader(string.IsNullOrWhiteSpace(submitSlug.AuthToken)
                ? string.Empty
                : submitSlug.AuthToken);

            //if (string.IsNullOrWhiteSpace(submitSlug.AuthToken))
            //{
            //    _gateway.ChangeAuthenticationHeader(string.Empty);
            //}
            //else
            //{
            //    _gateway.ChangeAuthenticationHeader(submitSlug.AuthToken);
            //}

            var response = await _gateway.PostAsync(submitSlug.URL, mappingEntity.Data);
            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException($"SubmitService::ProcessSubmission, An exception has occurred while attempting to call {submitSlug.URL}, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");
            }

            if (response.Content != null)
            {
                var content = await response.Content.ReadAsStringAsync() ?? string.Empty;
                reference = JsonConvert.DeserializeObject<string>(content);
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