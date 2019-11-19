using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.ViewModels;
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

        Task<FormBuilderViewModel> GenerateHtml(Page page, Dictionary<string, string> viewModel, FormSchema baseForm, IEnumerable<AddressSearchResult> addressSearchResults = null);

        void SaveAnswers(Dictionary<string, string> viewModel);

        bool CheckForStartPage(FormSchema form, Page page);
    }

    public class PageHelper : IPageHelper
    {
        private readonly IViewRender _viewRender;
        private readonly IElementHelper _elementHelper;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly DisallowedAnswerKeysConfiguration _disallowedKeys;

        public PageHelper(IViewRender viewRender, IElementHelper elementHelper, IDistributedCacheWrapper distributedCache, IOptions<DisallowedAnswerKeysConfiguration> disallowedKeys)
        {
            _viewRender = viewRender;
            _elementHelper = elementHelper;
            _distributedCache = distributedCache;
            _disallowedKeys = disallowedKeys.Value;
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

        public async Task<FormBuilderViewModel> GenerateHtml(Page page, Dictionary<string, string> viewModel, FormSchema baseForm, IEnumerable<AddressSearchResult> addressSearchResults = null)
        {
            FormBuilderViewModel formModel = new FormBuilderViewModel();
            if (page.PageURL.ToLower() != "success")
            {
                formModel.RawHTML += await _viewRender.RenderAsync("H1", new Element { Properties = new Property { Text = baseForm.Name } });
            }
            formModel.FeedbackForm = baseForm.FeedbackForm;

            CheckForDuplicateQuestionIDs(page);

            foreach (var element in page.Elements)
            {
                switch (element.Type)
                {
                    case EElementType.H1:
                        formModel.RawHTML += await _viewRender.RenderAsync("H1", element);
                        break;
                    case EElementType.H2:
                        formModel.RawHTML += await _viewRender.RenderAsync("H2", element);
                        break;
                    case EElementType.H3:
                        formModel.RawHTML += await _viewRender.RenderAsync("H3", element);
                        break;
                    case EElementType.H4:
                        formModel.RawHTML += await _viewRender.RenderAsync("H4", element);
                        break;
                    case EElementType.H5:
                        formModel.RawHTML += await _viewRender.RenderAsync("H5", element);
                        break;
                    case EElementType.H6:
                        formModel.RawHTML += await _viewRender.RenderAsync("H6", element);
                        break;
                    case EElementType.Img:
                        formModel.RawHTML += await _viewRender.RenderAsync("Img", element);
                        break;
                    case EElementType.P:
                        formModel.RawHTML += await _viewRender.RenderAsync("P", element);
                        break;
                    case EElementType.OL:
                        formModel.RawHTML += await _viewRender.RenderAsync("OL", element);
                        break;
                    case EElementType.UL:
                        formModel.RawHTML += await _viewRender.RenderAsync("UL", element);
                        break;
                    case EElementType.Span:
                        formModel.RawHTML += await _viewRender.RenderAsync("Span", element);
                        break;
                    case EElementType.InlineAlert:
                        formModel.RawHTML += await _viewRender.RenderAsync("InlineAlert", element);
                        _elementHelper.CheckIfLabelAndTextEmpty(element);
                        break;
                    case EElementType.Textbox:
                        _elementHelper.CheckForQuestionId(element);
                        element.Properties.Value = _elementHelper.CurrentValue(element, viewModel);
                        _elementHelper.CheckForLabel(element);
                        formModel.RawHTML += await _viewRender.RenderAsync("Textbox", element);
                        break;
                    case EElementType.Textarea:
                        _elementHelper.CheckForQuestionId(element);
                        element.Properties.Value = _elementHelper.CurrentValue(element, viewModel);
                        _elementHelper.CheckForLabel(element);
                        _elementHelper.CheckForMaxLength(element);
                        formModel.RawHTML += await _viewRender.RenderAsync("Textarea", element);
                        break;
                    case EElementType.Radio:
                        _elementHelper.CheckForQuestionId(element);
                        element.Properties.Value = _elementHelper.CurrentValue(element, viewModel);
                        _elementHelper.CheckForLabel(element);
                        _elementHelper.CheckForRadioOptions(element);
                        _elementHelper.ReCheckPreviousRadioOptions(element);
                        formModel.RawHTML += await _viewRender.RenderAsync("Radio", element);
                        break;
                    case EElementType.Button:
                        var viewData = new Dictionary<string, object> { { "displayAnchor", !CheckForStartPage(baseForm, page) } };
                        formModel.RawHTML += await _viewRender.RenderAsync("Button", element, viewData);
                        break;
                    case EElementType.Select:
                        _elementHelper.CheckForQuestionId(element);
                        element.Properties.Value = _elementHelper.CurrentValue(element, viewModel);
                        _elementHelper.ReSelectPreviousSelectedOptions(element);
                        _elementHelper.CheckForLabel(element);
                        _elementHelper.CheckForSelectOptions(element);
                        formModel.RawHTML += await _viewRender.RenderAsync("Select", element);
                        break;
                    case EElementType.Checkbox:
                        _elementHelper.CheckForQuestionId(element);
                        element.Properties.Value = _elementHelper.CurrentValue(element, viewModel);
                        _elementHelper.CheckForLabel(element);
                        _elementHelper.CheckForCheckBoxListValues(element);
                        formModel.RawHTML += await _viewRender.RenderAsync("Checkbox", element);
                        break;
                    case EElementType.DateInput:
                        _elementHelper.CheckForQuestionId(element);
                        element.Properties.Day = _elementHelper.CurrentDateValue(element, viewModel, "-day");
                        element.Properties.Month = _elementHelper.CurrentDateValue(element, viewModel, "-month");
                        element.Properties.Year = _elementHelper.CurrentDateValue(element, viewModel, "-year");
                        _elementHelper.CheckForLabel(element);
                        _elementHelper.CheckAllDateRestrictionsAreNotEnabled(element);
                        formModel.RawHTML += await _viewRender.RenderAsync("DateInput", element);
                        break;
                    case EElementType.Address:
                        formModel.RawHTML += await GenerateAddressHtml(viewModel, page, element, addressSearchResults);
                        break;
                    default:
                        break;
                }
            }

            return formModel;
        }

        private async Task<string> GenerateAddressHtml(Dictionary<string, string> viewModel, Page page, Element element, IEnumerable<AddressSearchResult> searchResults)
        {
            var postcodeKey = $"{element.Properties.QuestionId}-postcode";

            if (viewModel.ContainsKey("AddressStatus") && viewModel["AddressStatus"] == "Select" || viewModel.ContainsKey(postcodeKey) && !string.IsNullOrEmpty(viewModel[postcodeKey]))
            {
                element.Properties.Value = _elementHelper.CurrentValue(element, viewModel);
                return await _viewRender.RenderAsync("AddressSelect", new Tuple<Element, List<AddressSearchResult>>(element, searchResults.ToList()));
            }

            element.Properties.Value = _elementHelper.CurrentValue(element, viewModel);
            return await _viewRender.RenderAsync("AddressSearch", element);
        }

        public void SaveAnswers(Dictionary<string, string> viewModel)
        {
            var guid = viewModel["Guid"];
            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

            if (!string.IsNullOrEmpty(formData))
            {
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);
            }

            if (convertedAnswers.Pages != null && convertedAnswers.Pages.Any(_ => _.PageUrl == viewModel["Path"].ToLower()))
            {
                convertedAnswers.Pages = convertedAnswers.Pages.Where(_ => _.PageUrl != viewModel["Path"].ToLower()).ToList();
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
                PageUrl = viewModel["Path"].ToLower(),
                Answers = answers
            });
            convertedAnswers.Path = viewModel["Path"];
            convertedAnswers.AddressStatus = viewModel["AddressStatus"];

            _distributedCache.SetStringAsync(guid, JsonConvert.SerializeObject(convertedAnswers));
        }

        public bool CheckForStartPage(FormSchema form, Page page)
        {
            return form.StartPage == page.PageURL;
        }
    }
}
