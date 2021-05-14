using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.ContentFactory.PageFactory;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Services.PageService.Entities;
using Microsoft.Extensions.Options;

namespace form_builder.Services.AddAnotherService
{
    public class AddAnotherService : IAddAnotherService
    {
        private readonly IPageHelper _pageHelper;
        private readonly IPageFactory _pageContentFactory;
        private readonly FormConfiguration _disallowedKeys;

        public AddAnotherService(IPageHelper pageHelper, IPageFactory pageContentFactory, IOptions<FormConfiguration> disallowedKeys)
        {
            _pageHelper = pageHelper;
            _pageContentFactory = pageContentFactory;
            _disallowedKeys = disallowedKeys.Value;
        }

        public async Task<ProcessRequestEntity> ProcessAddAnother(
            Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path)
        {
            string removeKey = viewModel.Keys.FirstOrDefault(_ => _.Contains("remove"));
            var updatedViewModel = new Dictionary<string, dynamic>();

            if (!string.IsNullOrEmpty(removeKey))
            {
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
                            var splitQuestionId = item.Key.Split('_');
                            var currentQuestionIncrement = int.Parse(splitQuestionId[1]);
                            if (currentQuestionIncrement > incrementToRemove)
                            {
                                updatedViewModel.Add($"{splitQuestionId[0]}_{currentQuestionIncrement - 1}", item.Value);
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
            else
            {
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
                        subPath = "remove"
                    }
                };
            }

            var key = viewModel.Keys.FirstOrDefault(_ => _.Contains("addAnother"));

            if (currentPage.IsValid && string.IsNullOrEmpty(key))
            {
                return new ProcessRequestEntity
                {
                    Page = currentPage
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
                RedirectToAction = true,
                RedirectAction = "Index",
                RouteValues = new
                {
                    form = baseForm.BaseURL,
                    path,
                    subPath = "add-another"
                }
            };
        }
    }
}
