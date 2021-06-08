using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.ContentFactory.PageFactory;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Providers.StorageProvider;
using form_builder.Services.PageService.Entities;
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

        public AddAnotherService(IPageHelper pageHelper, IPageFactory pageContentFactory, IOptions<FormConfiguration> disallowedKeys, IDistributedCacheWrapper distributedCache)
        {
            _pageHelper = pageHelper;
            _pageContentFactory = pageContentFactory;
            _distributedCache = distributedCache;
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

        public Page ReplaceAddAnotherWithElements(Page currentPage, bool addEmptyFieldset, string sessionGuid)
        {
            var formData = _distributedCache.GetString(sessionGuid);
            var formAnswers = !string.IsNullOrEmpty(formData)
                ? JsonConvert.DeserializeObject<FormAnswers>(formData)
                : new FormAnswers { Pages = new List<PageAnswers>() };

            var pageAnswers = new List<Answers>();
            if (formAnswers.Pages != null &&
                formAnswers.Pages.FirstOrDefault(_ => _.PageSlug.Equals(currentPage.PageSlug)) != null)
            {
                pageAnswers = formAnswers.Pages.FirstOrDefault(_ => _.PageSlug.Equals(currentPage.PageSlug)).Answers;
            }

            var listOfFieldsetIncrements = pageAnswers.Select(answer => int.Parse(answer.QuestionId.Split('-')[1])).ToList();

            var currentIncrement = listOfFieldsetIncrements.Count > 0 ? listOfFieldsetIncrements.Max(_ => _) : 0;
            var maxFieldsetIncrement = listOfFieldsetIncrements.Count == 0 ? 0 : addEmptyFieldset ? currentIncrement + 1 : currentIncrement;
            var addAnotherElement = currentPage.Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.AddAnother));
            var indexOfAddAnother = currentPage.Elements.IndexOf(addAnotherElement);
            var addAnotherReplacementElements = GenerateListOfIncrementedElements(currentPage.Elements, maxFieldsetIncrement);

            currentPage.Elements.InsertRange(indexOfAddAnother, addAnotherReplacementElements);

            return currentPage;
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
                                    Action = "Remove",
                                    Controller = "AddAnother",
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
                            Name = "addAnotherFieldset",
                            Action = "AddAnother",
                            Controller = "AddAnother"
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
                    if (item.Key.EndsWith(incrementToRemove.ToString()))
                    {
                        answersToRemove.Add(item.Key, item.Value);
                    }
                    else
                    {
                        var splitQuestionId = item.Key.Split('-');
                        var currentQuestionIncrement = int.Parse(splitQuestionId[1]);
                        if (currentQuestionIncrement > incrementToRemove)
                        {
                            updatedViewModel.Add($"{splitQuestionId[0]}-{currentQuestionIncrement - 1}", item.Value);
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
            _pageHelper.SaveAnswers(updatedViewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);
        }

        public async Task<ProcessRequestEntity> ProcessAddAnother(
            Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path)
        {
            string removeKey = viewModel.Keys.FirstOrDefault(_ => _.Contains("remove"));
            bool addAnotherFieldset = viewModel.Keys.Any(_ => _.Equals("addAnotherFieldset"));

            if (!string.IsNullOrEmpty(removeKey))
            {
                RemoveFieldset(viewModel, currentPage, baseForm, guid, path, removeKey);
            }
            else
            {
                if (currentPage.IsValid)
                    _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);
            }

            if (!string.IsNullOrEmpty(removeKey))
            {
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

            if (currentPage.IsValid && addAnotherFieldset)
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

            if (!currentPage.IsValid)
            {
                var invalidFormModel = await _pageContentFactory.Build(currentPage, viewModel, baseForm, guid);

                return new ProcessRequestEntity
                {
                    Page = currentPage,
                    ViewModel = invalidFormModel
                };
            }

            return new ProcessRequestEntity
            {
                Page = currentPage
            };
        }
    }
}
