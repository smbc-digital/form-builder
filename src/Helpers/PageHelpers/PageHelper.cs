using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Helpers.Session;
using form_builder.Helpers.ViewRender;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Providers.Lookup;
using form_builder.Providers.StorageProvider;
using form_builder.Services.RetrieveExternalDataService.Entities;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Helpers.PageHelpers
{
    public class PageHelper : IPageHelper
    {
        private readonly IViewRender _viewRender;
        private readonly IActionHelper _actionHelper;
        private readonly IElementHelper _elementHelper;
        private readonly ISessionHelper _sessionHelper;
        private readonly IWebHostEnvironment _environment;
        private readonly FormConfiguration _disallowedKeys;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IEnumerable<ILookupProvider> _lookupProviders;
        private readonly DistributedCacheExpirationConfiguration _distributedCacheExpirationConfiguration;

        public PageHelper(IViewRender viewRender, IElementHelper elementHelper, IDistributedCacheWrapper distributedCache,
            IOptions<FormConfiguration> disallowedKeys, IWebHostEnvironment enviroment,
            IOptions<DistributedCacheExpirationConfiguration> distributedCacheExpirationConfiguration,
            ISessionHelper sessionHelper, IEnumerable<ILookupProvider> lookupProviders,
            IActionHelper actionHelper)
        {
            _viewRender = viewRender;
            _elementHelper = elementHelper;
            _distributedCache = distributedCache;
            _disallowedKeys = disallowedKeys.Value;
            _environment = enviroment;
            _distributedCacheExpirationConfiguration = distributedCacheExpirationConfiguration.Value;
            _sessionHelper = sessionHelper;
            _lookupProviders = lookupProviders;
            _actionHelper = actionHelper;
        }

        public async Task<FormBuilderViewModel> GenerateHtml(
            Page page,
            Dictionary<string, dynamic> viewModel,
            FormSchema baseForm,
            string guid,
            FormAnswers formAnswers,
            List<object> results = null)
        {
            var formModel = new FormBuilderViewModel();

            if (page.PageSlug.ToLower() != "success" && !page.HideTitle)
                formModel.RawHTML += await _viewRender
                    .RenderAsync("H1", new Element { Properties = new BaseProperty { Text = page.GetPageTitle() } });

            if (page.AllowAddAnother)
            {
                var subPath = viewModel["subPath"];

                var pageAnswers = new List<Answers>();
                if (formAnswers.Pages != null &&
                    formAnswers.Pages.FirstOrDefault(_ => _.PageSlug.Equals(page.PageSlug)) != null)
                {
                    pageAnswers = formAnswers.Pages.FirstOrDefault(_ => _.PageSlug.Equals(page.PageSlug)).Answers;
                }

                var listOfIncrementNumbers = new List<int>();
                foreach (var answer in pageAnswers)
                {
                    int pFrom = answer.QuestionId.IndexOf("[") + "[".Length;
                    int pTo = answer.QuestionId.LastIndexOf("]");

                    listOfIncrementNumbers.Add(int.Parse(answer.QuestionId.Substring(pFrom, pTo - pFrom)));
                }

                var currentIncrement = listOfIncrementNumbers.Count > 0 ? listOfIncrementNumbers.Max(_ => _) : 0;

                // somehow get the increment value ie. how many fieldsets do we need to render
                bool addAnother = subPath.Equals("add-another");

                var increment = listOfIncrementNumbers.Count == 0 ? 0 : addAnother ? currentIncrement + 1 : currentIncrement;
                // get the list of input elements required (ignore submit/continue)

                var inputElements = page.AddAnotherCompatibleElements.ToList();
                // for each fieldset, start a fieldset, then for each element add the element html inside the fieldset using the increment, end the fieldset
                string html = "";
                for (var i = 0; i <= increment; i++)
                {
                    html += $"<fieldset class='govuk-fieldset'><legend class='govuk-fieldset__legend govuk-fieldset__legend--m'>{page.AddAnotherFieldsetHeading}</legend>";
                    if (increment > 0)
                    {
                        html += $"<button data-prevent-double-click='true'data-disable-on-click = true class='govuk-button govuk-button--secondary' name='remove-{i}' id='remove-{i}' 'aria-describedby=remove' data-module='govuk-button'> Remove </button>";
                    }
                    foreach (var element in inputElements)
                    {
                        element.Properties.QuestionIdIncrement = i;
                        html += await element.RenderAsync(_viewRender, _elementHelper, guid, viewModel, page, baseForm,
                            _environment, formAnswers, results);
                    }

                    html += "</fieldset>";
                }
                // add the addAnother button
                html += $"<button data-prevent-double-click='true'data-disable-on-click = true class='govuk-button govuk-button--secondary' name='addAnother-{increment}' id='addAnother' 'aria-describedby=add-another' data-module='govuk-button'> Add another </button>";
                // add the submit button
                foreach (var element in page.Elements.Where(_ => _.Type.Equals(EElementType.Button)))
                {
                    html += await element.RenderAsync(_viewRender, _elementHelper, guid, viewModel, page, baseForm,
                        _environment, formAnswers, results);
                }

                // return the formModel
                formModel.RawHTML += html;
                return formModel;
            }
            
            foreach (var element in page.Elements)
            {
                if (!string.IsNullOrEmpty(element.Lookup) &&
                    element.Lookup.Equals(LookUpConstants.Dynamic))
                {
                    await AddDynamicOptions(element, formAnswers);
                }

                string html = await element.RenderAsync(_viewRender, _elementHelper, guid, viewModel, page, baseForm, _environment, formAnswers, results);

                if (element.Properties is not null && element.Properties.isConditionalElement)
                {
                    formModel.RawHTML = formModel.RawHTML.Replace($"{SystemConstants.CONDITIONAL_ELEMENT_REPLACEMENT}{element.Properties.QuestionId}", html);
                }
                else
                {
                    formModel.RawHTML += html;
                }
            }

            return formModel;
        }

        public async Task AddDynamicOptions(IElement element, FormAnswers formAnswers)
        {
            LookupSource submitDetails = element.Properties.LookupSources
                .SingleOrDefault(x => x.EnvironmentName
                .Equals(_environment.EnvironmentName, StringComparison.OrdinalIgnoreCase));

            if (submitDetails == null)
                throw new Exception("Dynamic lookup: No Environment Specific Details Found.");

            RequestEntity request = _actionHelper.GenerateUrl(submitDetails.URL, formAnswers);

            if (string.IsNullOrEmpty(submitDetails.Provider))
                throw new Exception("Dynamic lookup: No Query Details Found.");

            var lookupProvider = _lookupProviders.Get(submitDetails.Provider);
            if (lookupProvider == null)
                throw new Exception("Dynamic lookup: No Lookup Provider Found.");

            List<Option> lookupOptions = new();
            var session = _sessionHelper.GetSessionGuid();
            var cachedAnswers = _distributedCache.GetString(session);
            if (!string.IsNullOrEmpty(cachedAnswers))
            {
                var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);
                var lookUpCacheResults = convertedAnswers.FormData.SingleOrDefault(x => x.Key.Equals(request.Url, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrEmpty(lookUpCacheResults.Key) && lookUpCacheResults.Value != null)
                {
                    lookupOptions = JsonConvert.DeserializeObject<List<Option>>(JsonConvert.SerializeObject(lookUpCacheResults.Value));
                }
            }

            if (!lookupOptions.Any())
            {
                lookupOptions = await lookupProvider.GetAsync(request.Url, submitDetails.AuthToken);

                if (lookupOptions.Any())
                    SaveFormData(request.Url, lookupOptions, session, formAnswers.FormName);
            }

            if (!lookupOptions.Any())
                throw new Exception("Dynamic lookup: GetAsync cannot get IList<Options>.");

            element.Properties.Options.AddRange(lookupOptions);
        }

        public void SaveAnswers(Dictionary<string, dynamic> viewModel, string guid, string form, IEnumerable<CustomFormFile> files, bool isPageValid, bool appendMultipleFileUploadParts = false, bool isAddAnotherPage = false)
        {
            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };
            var currentPageAnswers = new PageAnswers();

            if (!string.IsNullOrEmpty(formData))
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            if (convertedAnswers.Pages != null && convertedAnswers.Pages.Any(_ => _.PageSlug == viewModel["Path"].ToLower()))
            {
                currentPageAnswers = convertedAnswers.Pages.Where(_ => _.PageSlug == viewModel["Path"].ToLower()).ToList().FirstOrDefault();
                convertedAnswers.Pages = convertedAnswers.Pages.Where(_ => _.PageSlug != viewModel["Path"].ToLower()).ToList();
            }

            var answers = new List<Answers>();

            foreach (var item in viewModel)
            {
                if (!_disallowedKeys.DisallowedAnswerKeys.Any(key => item.Key.Contains(key)))
                    if (isAddAnotherPage)
                    {
                        var keyAnswers = item.Value.ToString().Split(',');
                        for (var i = 0; i < keyAnswers.Length; i++)
                        {
                            if (currentPageAnswers.Answers != null && currentPageAnswers.Answers.Any(_ => _.QuestionId.Equals($"{item.Key}[{i}]")))
                            {
                                answers.Remove(new Answers {QuestionId = $"{item.Key}[{i}]"});
                            }

                            if (!string.IsNullOrEmpty(keyAnswers[i]))
                            {
                                answers.Add(new Answers { QuestionId = $"{item.Key}[{i}]", Response = keyAnswers[i] });
                            }
                        }
                    }
                    else
                    {
                        answers.Add(new Answers { QuestionId = item.Key, Response = item.Value });
                    }
            }

            if ((files == null || !files.Any()) && currentPageAnswers.Answers != null && currentPageAnswers.Answers.Any() && isPageValid && appendMultipleFileUploadParts)
                answers = currentPageAnswers.Answers;

            if (files != null && files.Any() && isPageValid)
                answers = SaveFormFileAnswers(answers, files, appendMultipleFileUploadParts, currentPageAnswers);

            convertedAnswers.Pages?.Add(new PageAnswers
            {
                PageSlug = viewModel["Path"].ToLower(),
                Answers = answers
            });

            convertedAnswers.Path = viewModel["Path"];
            convertedAnswers.FormName = form;

            _distributedCache.SetStringAsync(guid, JsonConvert.SerializeObject(convertedAnswers));
        }

        public void SaveCaseReference(string guid, string caseReference, bool isGenerated = false, string generatedRefereceMappingId = "GeneratedReference")
        {
            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

            if (!string.IsNullOrEmpty(formData))
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            if (isGenerated)
                convertedAnswers.AdditionalFormData.Add(generatedRefereceMappingId, caseReference);

            convertedAnswers.CaseReference = caseReference;
            _distributedCache.SetStringAsync(guid, JsonConvert.SerializeObject(convertedAnswers));
        }

        public void SaveFormData(string key, object value, string guid, string formName)
        {
            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

            if (!string.IsNullOrEmpty(formData))
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            if (convertedAnswers.FormData.ContainsKey(key))
                convertedAnswers.FormData.Remove(key);

            convertedAnswers.FormData.Add(key, value);
            convertedAnswers.FormName = formName;

            _distributedCache.SetStringAsync(guid, JsonConvert.SerializeObject(convertedAnswers));
        }

        public void SaveNonQuestionAnswers(Dictionary<string, object> values, string form, string path, string guid)
        {
            if (!values.Any())
                return;

            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = new FormAnswers 
            { 
                Pages = new List<PageAnswers>(), 
                FormName = form, 
                Path = path
            };

            if (!string.IsNullOrEmpty(formData))
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            values.ToList().ForEach((_) =>
            {
                if (convertedAnswers.AdditionalFormData.ContainsKey(_.Key))
                    convertedAnswers.AdditionalFormData.Remove(_.Key);

                convertedAnswers.AdditionalFormData.Add(_.Key, _.Value);
            });

            _distributedCache.SetStringAsync(guid, JsonConvert.SerializeObject(convertedAnswers));
        }

        public List<Answers> SaveFormFileAnswers(List<Answers> answers, IEnumerable<CustomFormFile> files, bool isMultipleFileUploadElementType, PageAnswers currentAnswersForFileUpload)
        {
            files.GroupBy(_ => _.QuestionId).ToList().ForEach(file =>
            {
                var fileUploadModel = new List<FileUploadModel>();
                var filsToAdd = file.ToList();
                if (isMultipleFileUploadElementType)
                {
                    var data = currentAnswersForFileUpload.Answers?.FirstOrDefault(_ => _.QuestionId.Equals(file.Key))?.Response;
                    if (data != null)
                    {
                        List<FileUploadModel> response = JsonConvert.DeserializeObject<List<FileUploadModel>>(data.ToString());
                        fileUploadModel.AddRange(response);
                        filsToAdd = filsToAdd.Where(_ => !response.Any(x => WebUtility.HtmlEncode(_.UntrustedOriginalFileName) == x.TrustedOriginalFileName)).ToList();
                    }
                }

                var keys = filsToAdd.Select(_ => $"file-{file.Key}-{Guid.NewGuid()}").ToArray();

                var fileContent = filsToAdd.Select(_ => _.Base64EncodedContent).ToList();

                for (int i = 0; i < fileContent.Count; i++)
                {
                    _distributedCache.SetStringAsync(keys[i], JsonConvert.SerializeObject(fileContent[i]), _distributedCacheExpirationConfiguration.FileUpload);
                }

                fileUploadModel.AddRange(filsToAdd.Select((_, index) => new FileUploadModel
                {
                    Key = keys[index],
                    TrustedOriginalFileName = WebUtility.HtmlEncode(_.UntrustedOriginalFileName),
                    UntrustedOriginalFileName = _.UntrustedOriginalFileName,
                    FileSize = _.Length
                }).ToList());

                if (answers.Exists(_ => _.QuestionId == file.Key))
                {
                    var fileUploadAnswer = answers.FirstOrDefault(_ => _.QuestionId == file.Key);
                    if (fileUploadAnswer != null)
                        fileUploadAnswer.Response = fileUploadModel;
                }
                else
                {
                    answers.Add(new Answers { QuestionId = file.Key, Response = fileUploadModel });
                }
            });

            return answers;
        }

        public Page GetPageWithMatchingRenderConditions(List<Page> pages)
        {
            var guid = _sessionHelper.GetSessionGuid();
            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = !string.IsNullOrEmpty(formData)
                ? JsonConvert.DeserializeObject<FormAnswers>(formData)
                : new FormAnswers { Pages = new List<PageAnswers>() };

            var answers = convertedAnswers.Pages.SelectMany(_ => _.Answers).ToDictionary(_ => _.QuestionId, _ => _.Response);

            return pages.FirstOrDefault(page => page.CheckPageMeetsConditions(answers));
        }
    }
}