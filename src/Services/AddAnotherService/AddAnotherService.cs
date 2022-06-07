using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.ContentFactory.PageFactory;
using form_builder.Enum;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Services.PageService.Entities;

namespace form_builder.Services.AddAnotherService
{
    public class AddAnotherService : IAddAnotherService
    {
        private readonly IPageHelper _pageHelper;
        private readonly IPageFactory _pageContentFactory;

        public AddAnotherService(IPageHelper pageHelper,
            IPageFactory pageContentFactory)
        {
            _pageHelper = pageHelper;
            _pageContentFactory = pageContentFactory;
        }

        public async Task<ProcessRequestEntity> ProcessAddAnother(
            Dictionary<string, dynamic> viewModel,
            Page dynamicCurrentPage,
            FormSchema baseForm,
            string guid,
            string path)
        {
            string removeKey = viewModel.Keys.FirstOrDefault(_ => _.Contains("remove"));
            bool addEmptyFieldset = viewModel.Keys.Any(_ => _.Equals(AddAnotherConstants.AddAnotherButtonKey));

            FormAnswers convertedFormAnswers = _pageHelper.GetSavedAnswers(guid);
            var maximumFieldsets = dynamicCurrentPage.Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.AddAnother)).Properties.MaximumFieldsets;

            if (dynamicCurrentPage.IsValid || !string.IsNullOrEmpty(removeKey))
            {
                var addAnotherElement = dynamicCurrentPage.Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.AddAnother));
                var formDataIncrementKey = $"{AddAnotherConstants.IncrementKeyPrefix}{addAnotherElement.Properties.QuestionId}";
                var currentIncrement = convertedFormAnswers.FormData.ContainsKey(formDataIncrementKey) ? int.Parse(convertedFormAnswers.FormData.GetValueOrDefault(formDataIncrementKey).ToString()) : 1;

                if (addEmptyFieldset && currentIncrement >= maximumFieldsets)
                    throw new ApplicationException("AddAnotherService::ProcessAddAnother, maximum number of fieldsets exceeded");

                if (addEmptyFieldset)
                    currentIncrement++;

                if (!string.IsNullOrEmpty(removeKey))
                    currentIncrement--;

                if (currentIncrement.Equals(1) && addAnotherElement.Properties.Elements.All(
                        subElement => subElement.Properties.Optional && 
                        string.IsNullOrEmpty(viewModel[$"{subElement.Properties.QuestionId}:{currentIncrement}:"])))
                {
                    currentIncrement = 0;
                }

                _pageHelper.SaveFormData(formDataIncrementKey, currentIncrement, guid, baseForm.BaseURL);
            }

            if (!string.IsNullOrEmpty(removeKey))
            {
                _pageHelper.RemoveFieldset(viewModel, baseForm.BaseURL, guid, path, removeKey);
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

            _pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, dynamicCurrentPage.IsValid);

            if (!dynamicCurrentPage.IsValid)
            {
                var invalidFormModel = await _pageContentFactory.Build(dynamicCurrentPage, viewModel, baseForm, guid);

                return new ProcessRequestEntity
                {
                    Page = dynamicCurrentPage,
                    ViewModel = invalidFormModel
                };
            }

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
    }
}
