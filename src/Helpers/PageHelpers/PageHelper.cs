using form_builder.Configuration;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.StorageProvider;
using form_builder.Services.PageService.Entities;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Addresses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Helpers.PageHelpers
{
    public interface IPageHelper
    {
        void CheckForDuplicateQuestionIDs(Page page);
        Task<FormBuilderViewModel> GenerateHtml(Page page, Dictionary<string, string> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressSearchResults = null);
        void SaveAnswers(Dictionary<string, string> viewModel, string guid);
        Task<ProcessPageEntity> ProcessStreetAndAddressJourney(string journey, Page currentPage, Dictionary<string, string> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressResults, bool isAddressJourney);
    }

    public class PageHelper : IPageHelper
    {
        private readonly IViewRender _viewRender;
        private readonly IElementHelper _elementHelper;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly DisallowedAnswerKeysConfiguration _disallowedKeys;
        private readonly IHostingEnvironment _enviroment;
        public PageHelper(IViewRender viewRender, IElementHelper elementHelper, IDistributedCacheWrapper distributedCache, IOptions<DisallowedAnswerKeysConfiguration> disallowedKeys, IHostingEnvironment enviroment)
        {
            _viewRender = viewRender;
            _elementHelper = elementHelper;
            _distributedCache = distributedCache;
            _disallowedKeys = disallowedKeys.Value;
            _enviroment = enviroment;
        }

        public void CheckForDuplicateQuestionIDs(Page page)
        {
            var numberOfDuplicates = page.Elements.GroupBy(x => x.Properties.QuestionId + x.Properties.Text + x.Type)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key)
                .ToList();

            if (numberOfDuplicates.Count > 0)
            {
                throw new Exception("Question id, text or type is not unique.");
            }
        }

        public async Task<FormBuilderViewModel> GenerateHtml(Page page, Dictionary<string, string> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressAndStreetSearchResults = null)
        {
            FormBuilderViewModel formModel = new FormBuilderViewModel();
            if (page.PageSlug.ToLower() != "success")
            {
                formModel.RawHTML += await _viewRender.RenderAsync("H1", new Element { Properties = new Property { Text = baseForm.FormName } });
            }
            formModel.FeedbackForm = baseForm.FeedbackForm;

            CheckForDuplicateQuestionIDs(page);

            foreach (var element in page.Elements)
            {
                formModel.RawHTML += await element.RenderAsync(_viewRender, _elementHelper, guid, addressAndStreetSearchResults, viewModel, page, baseForm, _enviroment);
            }

            return formModel;
        }

        public void SaveAnswers(Dictionary<string, string> viewModel, string guid)
        {
            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

            if (!string.IsNullOrEmpty(formData))
            {
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);
            }

            if (convertedAnswers.Pages != null && convertedAnswers.Pages.Any(_ => _.PageSlug == viewModel["Path"].ToLower()))
            {
                convertedAnswers.Pages = convertedAnswers.Pages.Where(_ => _.PageSlug != viewModel["Path"].ToLower()).ToList();
            }

            var answers = new List<Answers>();

            foreach (var item in viewModel)
            {
                if (!_disallowedKeys.DisallowedAnswerKeys.Contains(item.Key))
                {
                    answers.Add(new Answers { QuestionId = item.Key, Response = item.Value });
                }
            }

            convertedAnswers.Pages.Add(new PageAnswers
            {
                PageSlug = viewModel["Path"].ToLower(),
                Answers = answers
            });

            convertedAnswers.Path = viewModel["Path"];

            _distributedCache.SetStringAsync(guid, JsonConvert.SerializeObject(convertedAnswers));
        }

        public async Task<ProcessPageEntity> ProcessStreetAndAddressJourney(string journey, Page currentPage, Dictionary<string, string> viewModel, FormSchema baseForm, string guid, List<AddressSearchResult> addressResults, bool isAddressJourney)
        {
            switch (journey)
            {
                case "Search":
                    try
                    {
                        var adddressViewModel = await GenerateHtml(currentPage, viewModel, baseForm, guid, addressResults);
                        adddressViewModel.AddressStatus = "Select";
                        adddressViewModel.StreetStatus = "Select";
                        adddressViewModel.FormName = baseForm.FormName;

                        return new ProcessPageEntity
                        {
                            Page = currentPage,
                            ViewModel = adddressViewModel,
                            UseGeneratedViewModel = true,
                            ViewName = isAddressJourney ? "../Address/Index" : "../Street/Index"
                        };
                    }
                    catch (Exception e)
                    {
                        throw new ApplicationException($"AddressController: An exception has occured while attempting to generate Html, Exception: {e.Message}");
                    };
                case "Select":
                    return new ProcessPageEntity
                    {
                        Page = currentPage
                    };
                default:
                    throw new ApplicationException($"AddressController: Unknown journey type");
            }
        }

    }
}