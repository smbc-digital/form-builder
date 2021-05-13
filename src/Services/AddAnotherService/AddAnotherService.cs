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

        public async Task<ProcessRequestEntity> ProcessAddAnother(Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path)
        {
            string removeKey = viewModel.Keys.FirstOrDefault(_ => _.Contains("remove"));

            if (!string.IsNullOrEmpty(removeKey))
            {
                var incrementToRemove = int.Parse(removeKey.Split('-')[1]);

                var questions = new Dictionary<string, dynamic>();
                foreach (var item in viewModel)
                {
                    if (!_disallowedKeys.DisallowedAnswerKeys.Any(key => item.Key.Contains(key)))
                    {
                        questions[item.Key] = item.Value;
                    }
                }

                foreach (var question in questions)
                {
                    var questionAnswers = question.Value.ToString().Split(',');
                    var newList = new List<string>();
                    for (var j = 0; j < questionAnswers.Length; j++)
                    {
                        if (j != incrementToRemove)
                        {
                            newList.Add(questionAnswers[j]);
                        }
                    }

                    var newAnswer = string.Join(',', newList);
                    viewModel[question.Key] = newAnswer;
                }
            }

            _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid, false, currentPage.AllowAddAnother);

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

            string key = viewModel.Keys.FirstOrDefault(_ => _.Contains("addAnother"));

            if (currentPage.IsValid && string.IsNullOrEmpty(key))
            {
                return new ProcessRequestEntity
                {
                    Page = currentPage
                };
            }

            int increment = 0;

            if (!string.IsNullOrEmpty(key))
            {
                var splitKey = key.Split('-');
                increment = int.Parse(splitKey[1]);
            }

            if (!currentPage.IsValid)
            {
                viewModel.Add("increment", increment);

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
