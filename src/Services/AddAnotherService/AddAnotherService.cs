using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.ContentFactory.PageFactory;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Factories.Schema;
using form_builder.Factories.Transform.AddAnother;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Providers.StorageProvider;
using form_builder.Services.PageService.Entities;
using form_builder.Validators;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace form_builder.Services.AddAnotherService
{
    public class AddAnotherService : IAddAnotherService
    {
        private readonly IPageHelper _pageHelper;
        private readonly IPageFactory _pageContentFactory;
        private readonly FormConfiguration _disallowedKeys;
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IAddAnotherSchemaTransformFactory _addAnotherSchemaTransformFactory;
        private readonly ISchemaFactory _schemaFactory;

        public AddAnotherService(IPageHelper pageHelper, 
            IPageFactory pageContentFactory, 
            IOptions<FormConfiguration> disallowedKeys, 
            IDistributedCacheWrapper distributedCache, 
            IAddAnotherSchemaTransformFactory addAnotherSchemaTransformFactory, 
            ISchemaFactory schemaFactory)
        {
            _pageHelper = pageHelper;
            _pageContentFactory = pageContentFactory;
            _distributedCache = distributedCache;
            _addAnotherSchemaTransformFactory = addAnotherSchemaTransformFactory;
            _schemaFactory = schemaFactory;
            _disallowedKeys = disallowedKeys.Value;
        }

        public Page GenerateAddAnotherElementsForValidation(Page currentPage, Dictionary<string, dynamic> viewModel)
        {
            var listOfFieldsetIncrements = new List<int>();
            foreach (var answer in viewModel)
            {
                if (!_disallowedKeys.DisallowedAnswerKeys.Any(key => answer.Key.Contains(key)))
                {
                    listOfFieldsetIncrements.Add(int.Parse(answer.Key.Split('-')[1]));
                }
            }

            var maxFieldsetIncrement = listOfFieldsetIncrements.Count > 0 ? listOfFieldsetIncrements.Max(_ => _) : 0;
            var addAnotherElement = currentPage.Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.AddAnother));
            var indexOfAddAnother = currentPage.Elements.IndexOf(addAnotherElement);
            var addAnotherReplacementElements = GenerateListOfIncrementedElements(currentPage.Elements, maxFieldsetIncrement);

            currentPage.Elements.InsertRange(indexOfAddAnother, addAnotherReplacementElements);

            return currentPage;
        }

        public (FormSchema dynamicFormSchema, Page dynamicCurrentPage) GetDynamicPageFromFormData(Page currentPage, string guid)
        {
            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

            if (!string.IsNullOrEmpty(formData))
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            FormSchema dynamicFormSchema = JsonConvert.DeserializeObject<FormSchema>(convertedAnswers.FormData["dynamicFormSchema"].ToString());
            Page dynamicCurrentPage =
                dynamicFormSchema.Pages.FirstOrDefault(_ => _.PageSlug.Equals(currentPage.PageSlug));

            return (dynamicFormSchema, dynamicCurrentPage);
        }

        private IEnumerable<IElement> GenerateListOfIncrementedElements(IReadOnlyCollection<IElement> currentPageElements, int maxFieldsetIncrement)
        {
            var addAnotherElement = currentPageElements.FirstOrDefault(_ => _.Type.Equals(EElementType.AddAnother));
            var addAnotherReplacementElements = new List<IElement>();

            foreach (var pageElement in currentPageElements)
            {
                if (pageElement.Type.Equals(EElementType.AddAnother))
                {
                    for (var i = 0; i <= maxFieldsetIncrement; i++)
                    {
                        addAnotherReplacementElements.Add(new FieldsetOpen
                        {
                            Properties = new BaseProperty
                            {
                                Label = addAnotherElement.Properties.Label
                            }
                        });

                        if (maxFieldsetIncrement > 0)
                        {
                            addAnotherReplacementElements.Add(new PostbackButton
                            {
                                Properties = new BaseProperty
                                {
                                    Label = "Remove",
                                    Name = $"remove-{i}"
                                }
                            });
                        }

                        foreach (var element in addAnotherElement.Properties.Elements)
                        {
                            var incrementedElement = JsonConvert.DeserializeObject<IElement>(JsonConvert.SerializeObject(element));
                            incrementedElement.Properties.QuestionId = $"{element.Properties.QuestionId}-{i}";
                            addAnotherReplacementElements.Add(incrementedElement);
                        }

                        addAnotherReplacementElements.Add(new FieldsetClose
                        {
                            Properties = new BaseProperty
                            {
                                Text = string.Empty
                            }
                        });
                    }

                    addAnotherReplacementElements.Add(new PostbackButton
                    {
                        Properties = new BaseProperty
                        {
                            Label = "Add another",
                            Name = "addAnotherFieldset"
                        }
                    });
                }
            }

            return addAnotherReplacementElements;
        }

        private void RemoveFieldset(Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path,
            string removeKey)
        {
            var updatedViewModel = new Dictionary<string, dynamic>();

            var incrementToRemove = int.Parse(removeKey.Split('-')[1]);
            var answersToRemove = new Dictionary<string, dynamic>();
            foreach (var item in viewModel)
            {
                if (!_disallowedKeys.DisallowedAnswerKeys.Any(key => item.Key.Contains(key)))
                {
                    var splitQuestionId = item.Key.Split('-');
                    var currentQuestionIncrement = int.Parse(splitQuestionId[1]);
                    if (currentQuestionIncrement == incrementToRemove)
                    {
                        answersToRemove.Add(item.Key, item.Value);
                    }
                    else
                    {
                        if (currentQuestionIncrement > incrementToRemove)
                        {
                            splitQuestionId[1] = $"{currentQuestionIncrement - 1}";
                            var newQuestionId = string.Join('-', splitQuestionId);

                            updatedViewModel.Add(newQuestionId, item.Value);
                        }
                        else
                        {
                            updatedViewModel.Add(item.Key, item.Value);
                        }
                    }
                }
                else
                {
                    updatedViewModel.Add(item.Key, item.Value);
                }
            }

            _pageHelper.RemoveAnswers(answersToRemove, guid, path);
            _pageHelper.SaveAnswers(updatedViewModel, guid, baseForm.BaseURL, null, true);
        }

        public async Task<ProcessRequestEntity> ProcessAddAnother(
            Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path,
            IEnumerable<IElementValidator> validators)
        {
            string removeKey = viewModel.Keys.FirstOrDefault(_ => _.Contains("remove"));
            bool addEmptyFieldset = viewModel.Keys.Any(_ => _.Equals("addAnotherFieldset"));

            var (dynamicFormSchema, dynamicCurrentPage) = GetDynamicPageFromFormData(currentPage, guid);

            if ((addEmptyFieldset && currentPage.IsValid) || !string.IsNullOrEmpty(removeKey))
            {
                var baseFormAddAnotherElement = dynamicFormSchema.GetPage(_pageHelper, path).Elements
                    .FirstOrDefault(_ => _.Type.Equals(EElementType.AddAnother));

                if (addEmptyFieldset)
                    baseForm.GetPage(_pageHelper, path).Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.AddAnother)).Properties.CurrentNumberOfFieldsets = 
                        baseFormAddAnotherElement.Properties.CurrentNumberOfFieldsets + 1;

                if (!string.IsNullOrEmpty(removeKey))
                    baseForm.GetPage(_pageHelper, path).Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.AddAnother)).Properties.CurrentNumberOfFieldsets =
                        baseFormAddAnotherElement.Properties.CurrentNumberOfFieldsets - 1;

                var updatedFormSchema = _addAnotherSchemaTransformFactory.Transform(baseForm);
                _pageHelper.SaveFormData("dynamicFormSchema", updatedFormSchema, guid, baseForm.BaseURL);
            }
            
            if (!string.IsNullOrEmpty(removeKey))
            {
                RemoveFieldset(viewModel, dynamicCurrentPage, baseForm, guid, path, removeKey);
                return new ProcessRequestEntity
                {
                    RedirectToAction = true,
                    RedirectAction = "Index",
                    RouteValues = new
                    {
                        form = baseForm.BaseURL,
                        path,
                    }
                };
            }

            dynamicCurrentPage.Validate(viewModel, validators, dynamicFormSchema);

            if (!dynamicCurrentPage.IsValid)
            {
                var invalidFormModel = await _pageContentFactory.Build(dynamicCurrentPage, viewModel, baseForm, guid);

                return new ProcessRequestEntity
                {
                    Page = currentPage,
                    ViewModel = invalidFormModel
                };
            }

            _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, dynamicCurrentPage.IsValid);

            if (dynamicCurrentPage.IsValid && addEmptyFieldset)
            {
                return new ProcessRequestEntity
                {
                    RedirectToAction = true,
                    RedirectAction = "Index",
                    RouteValues = new
                    {
                        form = baseForm.BaseURL,
                        path
                    },
                    TempData = "addAnotherFieldset"
                };
            }

            return new ProcessRequestEntity
            {
                Page = dynamicCurrentPage
            };
        }
    }
}
