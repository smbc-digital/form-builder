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
using System.Threading.Tasks;
using form_builder.Services.MappingService.Entities;
using Microsoft.AspNetCore.Hosting;
using form_builder.Extensions;
using System.Linq;
using form_builder.Enum;
using Microsoft.AspNetCore.Http;

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

        private readonly IPageHelper _pageHelper;

        private readonly ISessionHelper _sessionHelper;

        private readonly ILogger<SubmitService> _logger;

        private readonly IHostingEnvironment _environment;

        private readonly IHttpContextAccessor _httpContextAccessor;


        public SubmitService(ILogger<SubmitService> logger, IDistributedCacheWrapper distributedCache, IGateway gateway, IPageHelper pageHelper, ISessionHelper sessionHelper, IHostingEnvironment environment, IHttpContextAccessor httpContextAccessor)
        {
            _distributedCache = distributedCache;
            _gateway = gateway;
            _pageHelper = pageHelper;
            _sessionHelper = sessionHelper;
            _logger = logger;
            _environment = environment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<SubmitServiceEntity> ProcessSubmission(MappingEntity mappingEntity, string form, string sessionGuid)
        {
            var reference = string.Empty;

            var currentPage = mappingEntity.BaseForm.GetPage(mappingEntity.FormAnswers.Path);
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
                throw new ApplicationException($"SubmitService::ProcessSubmission, An exception has occured while attemping to call {submitSlug.URL}, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");
            }

            if (response.Content != null)
            {
                var content = await response.Content.ReadAsStringAsync() ?? string.Empty;
                reference = JsonConvert.DeserializeObject<string>(content);
            }

            var formFileUploadElements = mappingEntity.BaseForm.Pages.SelectMany(_ => _.Elements)
                .Where(_ => _.Type == EElementType.FileUpload)
                .ToList();

            if (formFileUploadElements.Count > 0)
            {
                formFileUploadElements.ForEach(_ =>
                {
                    _distributedCache.Remove($"file-{_.Properties.QuestionId}");
                });
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

            var viewModel = await _pageHelper.GenerateHtml(page, new Dictionary<string, dynamic>(), mappingEntity.BaseForm, sessionGuid);
            viewModel.BaseURL = mappingEntity.BaseForm.BaseURL;
            viewModel.StartPageSlug = mappingEntity.BaseForm.StartPageSlug;

            var startFormUrl = $"https://{_httpContextAccessor.HttpContext.Request.Host}/{viewModel.BaseURL}/{viewModel.StartPageSlug}";
            viewModel.StartFormUrl = startFormUrl;

            var success = new Success
            {
                FormName = mappingEntity.BaseForm.FormName,
                Reference = reference,
                FormAnswers = mappingEntity.FormAnswers,
                PageContent = viewModel.RawHTML,
                SecondaryHeader = page.Title,
                StartFormUrl = viewModel.StartFormUrl
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
            var currentPage = mappingEntity.BaseForm.GetPage(mappingEntity.FormAnswers.Path);

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
                    throw new ApplicationException($"SubmitService::PaymentSubmission, Gateway {postUrl} responded with empty reference");
                }

                return JsonConvert.DeserializeObject<string>(content);
            }

            throw new ApplicationException($"SubmitService::PaymentSubmission, An exception has occured when response content from {postUrl} is null, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");
        }
    }
}