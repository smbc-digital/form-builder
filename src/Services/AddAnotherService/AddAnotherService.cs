using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.ContentFactory.PageFactory;
using form_builder.Enum;
using form_builder.Factories.Schema;
using form_builder.Factories.Transform.AddAnother;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
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

        public AddAnotherService(IPageHelper pageHelper, 
            IPageFactory pageContentFactory, 
            IOptions<FormConfiguration> disallowedKeys, 
            IDistributedCacheWrapper distributedCache, 
            IAddAnotherSchemaTransformFactory addAnotherSchemaTransformFactory)
        {
            _pageHelper = pageHelper;
            _pageContentFactory = pageContentFactory;
            _distributedCache = distributedCache;
            _addAnotherSchemaTransformFactory = addAnotherSchemaTransformFactory;
            _disallowedKeys = disallowedKeys.Value;
        }

        public (FormSchema dynamicFormSchema, Page dynamicCurrentPage) GetDynamicFormSchema(Page currentPage, string guid)
        {
            var formData = _distributedCache.GetString(guid);
            var convertedAnswers = new FormAnswers { Pages = new List<PageAnswers>() };

            if (!string.IsNullOrEmpty(formData))
                convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);

            FormSchema dynamicFormSchema = JsonConvert.DeserializeObject<FormSchema>(convertedAnswers.FormData["dynamicFormSchema"].ToString());
            Page dynamicCurrentPage = dynamicFormSchema.GetPage(_pageHelper, currentPage.PageSlug);

            return (dynamicFormSchema, dynamicCurrentPage);
        }

        

        public async Task<ProcessRequestEntity> ProcessAddAnother(
            Dictionary<string, dynamic> viewModel,
            Page dynamicCurrentPage,
            FormSchema baseForm,
            string guid,
            string path,
            FormSchema dynamicFormSchema)
        {
            string removeKey = viewModel.Keys.FirstOrDefault(_ => _.Contains("remove"));
            bool addEmptyFieldset = viewModel.Keys.Any(_ => _.Equals("addAnotherFieldset"));

            if ((addEmptyFieldset && dynamicCurrentPage.IsValid) || !string.IsNullOrEmpty(removeKey))
            {
                var dynamicAddAnotherElement = dynamicCurrentPage.Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.AddAnother));

                if (addEmptyFieldset)
                    baseForm.GetPage(_pageHelper, path).Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.AddAnother)).Properties.CurrentNumberOfFieldsets =
                        dynamicAddAnotherElement.Properties.CurrentNumberOfFieldsets + 1;

                if (!string.IsNullOrEmpty(removeKey))
                    baseForm.GetPage(_pageHelper, path).Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.AddAnother)).Properties.CurrentNumberOfFieldsets =
                        dynamicAddAnotherElement.Properties.CurrentNumberOfFieldsets - 1;

                var updatedFormSchema = _addAnotherSchemaTransformFactory.Transform(baseForm);
                _pageHelper.SaveFormData("dynamicFormSchema", updatedFormSchema, guid, baseForm.BaseURL);
            }
            
            if (!string.IsNullOrEmpty(removeKey))
            {
                RemoveFieldset(viewModel, baseForm.BaseURL, guid, path, removeKey);
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

            if (!dynamicCurrentPage.IsValid)
            {
                var invalidFormModel = await _pageContentFactory.Build(dynamicCurrentPage, viewModel, baseForm, guid);

                return new ProcessRequestEntity
                {
                    Page = dynamicCurrentPage,
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
                    }
                };
            }

            return new ProcessRequestEntity
            {
                Page = dynamicCurrentPage
            };
        }

        private void RemoveFieldset(Dictionary<string, dynamic> viewModel,
            string form,
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
            _pageHelper.SaveAnswers(updatedViewModel, guid, form, null, true);
        }
    }
}
