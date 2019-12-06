using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Services.SubmitService.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StockportGovUK.AspNetCore.Gateways;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

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

        private readonly IPageHelper _pageHelper;

        private readonly ISessionHelper _sessionHelper;

        private readonly ILogger<SubmitService> _logger;

        public SubmitService(ILogger<SubmitService> logger, IDistributedCacheWrapper distributedCache, ISchemaProvider schemaProvider, IGateway gateway, IPageHelper pageHelper, ISessionHelper sessionHelper)
        {
            _distributedCache = distributedCache;
            _schemaProvider = schemaProvider;
            _gateway = gateway;
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
            var postData = CreatePostData(convertedAnswers);
            var reference = string.Empty;

            var response = await _gateway.PostAsync(postUrl, postData);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException($"HomeController, Submit: An exception has occured while attemping to call {postUrl}, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");
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

        private PostData CreatePostData(FormAnswers formAnswers)
        {
            var postData = new PostData
            {
                Form = formAnswers.FormName,
                Answers = new List<Answers>()
            };

            if (formAnswers.Pages == null)
            {
                return postData;
            }

            foreach (var page in formAnswers.Pages)
            {
                foreach (var a in page.Answers)
                {
                    postData.Answers.Add(a);
                }
            }

            return postData;
        }
    }
}
