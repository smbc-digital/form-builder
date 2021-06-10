using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.ContentFactory.PageFactory;
using form_builder.Enum;
using form_builder.Factories.Transform.AddAnother;
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
        private readonly IAddAnotherSchemaTransformFactory _addAnotherSchemaTransformFactory;

        public AddAnotherService(IPageHelper pageHelper, 
            IPageFactory pageContentFactory, 
            IOptions<FormConfiguration> disallowedKeys,
            IAddAnotherSchemaTransformFactory addAnotherSchemaTransformFactory)
        {
            _pageHelper = pageHelper;
            _pageContentFactory = pageContentFactory;
            _addAnotherSchemaTransformFactory = addAnotherSchemaTransformFactory;
            _disallowedKeys = disallowedKeys.Value;
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
