using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
            bool addEmptyFieldset = viewModel.Keys.Any(_ => _.Equals("addAnotherFieldset"));

            FormAnswers convertedFormAnswers = _pageHelper.GetSavedAnswers(guid);

            if ((addEmptyFieldset && dynamicCurrentPage.IsValid) || !string.IsNullOrEmpty(removeKey))
            {
                var formDataIncrementKey = $"addAnotherFieldset-{dynamicCurrentPage.Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.AddAnother)).Properties.QuestionId}";
                var currentIncrement = convertedFormAnswers.FormData.ContainsKey(formDataIncrementKey) ? int.Parse(convertedFormAnswers.FormData.GetValueOrDefault(formDataIncrementKey).ToString()) : 0;

                if (addEmptyFieldset)
                    currentIncrement++;

                if (!string.IsNullOrEmpty(removeKey))
                    currentIncrement--;
                
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
    }
}
