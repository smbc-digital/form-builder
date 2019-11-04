﻿using form_builder.Configuration;
using form_builder.Enum;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models;
using form_builder.Providers;
using form_builder.ViewModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.Helpers.PageHelpers
{
    public interface IPageHelper
    {
        void CheckForDuplicateQuestionIDs(Page page);
        Task<FormBuilderViewModel> GenerateHtml(Page page, Dictionary<string, string> viewModel, FormSchema baseForm);
        void SaveAnswers(Dictionary<string, string> viewModel);
    }
    
    public class PageHelper : IPageHelper
    {
        private readonly IViewRender _viewRender;
        private readonly IElementHelper _elementHelper;
        private readonly ICacheProvider _cacheProvider;
        private readonly DisallowedAnswerKeysConfiguration _disallowedKeys;

        public PageHelper(IViewRender viewRender, IElementHelper elementHelper, ICacheProvider cacheProvider, IOptions<DisallowedAnswerKeysConfiguration> disallowedKeys)
        {
            _viewRender = viewRender;
            _elementHelper = elementHelper;
            _cacheProvider = cacheProvider;
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

        public async Task<FormBuilderViewModel> GenerateHtml(Page page, Dictionary<string, string> viewModel, FormSchema baseForm)
        {
            FormBuilderViewModel formModel = new FormBuilderViewModel();
            formModel.RawHTML += await _viewRender.RenderAsync("H1", new Element { Properties = new Property { Text = baseForm.Name } });
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
                    case EElementType.Textbox:
                        element.Properties.Value = _elementHelper.CurrentValue(element, viewModel);
                        _elementHelper.CheckForLabel(element, viewModel);
                        formModel.RawHTML += await _viewRender.RenderAsync("Textbox", element);
                        break;
                    case EElementType.Textarea:
                        element.Properties.Value = _elementHelper.CurrentValue(element, viewModel);
                        formModel.RawHTML += await _viewRender.RenderAsync("Textarea", element);
                        break;
                    case EElementType.Radio:
                        element.Properties.Value = _elementHelper.CurrentValue(element, viewModel);
                        formModel.RawHTML += await _viewRender.RenderAsync("Radio", element);
                        break;
                    case EElementType.Button:
                        formModel.RawHTML += await _viewRender.RenderAsync("Button", element);
                        break;
                    default:
                        break;
                }
            }

            return formModel;
        }

        public void SaveAnswers(Dictionary<string, string> viewModel)
        {
            var guid = viewModel["Guid"];
            var formData = _cacheProvider.GetString(guid);
            var convertedAnswers = new List<FormAnswers>();

            if (!string.IsNullOrEmpty(formData))
            {
                convertedAnswers = JsonConvert.DeserializeObject<List<FormAnswers>>(formData);
            }

            if (convertedAnswers.Any(_ => _.PageUrl == viewModel["Path"].ToLower()))
            {
                convertedAnswers = convertedAnswers.Where(_ => _.PageUrl != viewModel["Path"].ToLower())
                                                    .ToList();
            }

            var answers = new List<Answers>();

            foreach (var item in viewModel)
            {
                if (!_disallowedKeys.DisallowedAnswerKeys.Contains(item.Key))
                {
                    answers.Add(new Answers { QuestionId = item.Key, Response = item.Value });
                }
            }

            convertedAnswers.Add(new FormAnswers
            {
                PageUrl = viewModel["Path"].ToLower(),
                Answers = answers
            });

            _cacheProvider.SetString(guid, JsonConvert.SerializeObject(convertedAnswers), 30);
        }
    }
}
