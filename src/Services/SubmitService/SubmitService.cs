using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Providers.StorageProvider;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;
using System;
using System.Threading.Tasks;
using form_builder.Services.MappingService.Entities;
using Microsoft.AspNetCore.Hosting;
using form_builder.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using form_builder.Configuration;

namespace form_builder.Services.SubmtiService
{
    public interface ISubmitService
    {
        Task<string> ProcessSubmission(MappingEntity mappingEntity, string form, string sessionGuid);
        Task<string> PaymentSubmission(MappingEntity mappingEntity, string form, string sessionGuid);

    }
    public class SubmitService : ISubmitService
    {
        private readonly IDistributedCacheWrapper _distributedCache;

        private readonly IGateway _gateway;

        private readonly IPageHelper _pageHelper;

        private readonly ISessionHelper _sessionHelper;

        private readonly ILogger<SubmitService> _logger;

        private readonly IHostingEnvironment _environment;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly DistributedCacheExpirationConfiguration _distrbutedCacheExpirationConfiguration;

        private readonly SubmissionServiceConfiguration _submissionServiceConfiguration;


        public SubmitService(ILogger<SubmitService> logger, IDistributedCacheWrapper distributedCache, IGateway gateway, IPageHelper pageHelper, ISessionHelper sessionHelper, IHostingEnvironment environment, IHttpContextAccessor httpContextAccessor, IOptions<DistributedCacheExpirationConfiguration> distrbutedCacheExpirationConfiguration, IOptions<SubmissionServiceConfiguration> submissionServiceConfiguration)
        {
            _distributedCache = distributedCache;
            _gateway = gateway;
            _pageHelper = pageHelper;
            _sessionHelper = sessionHelper;
            _logger = logger;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
            _distrbutedCacheExpirationConfiguration = distrbutedCacheExpirationConfiguration.Value;
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

            if (string.IsNullOrWhiteSpace(submitSlug.AuthToken))
            {
                _gateway.ChangeAuthenticationHeader(string.Empty);
            }
            else
            {
                _gateway.ChangeAuthenticationHeader(submitSlug.AuthToken);
            }

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