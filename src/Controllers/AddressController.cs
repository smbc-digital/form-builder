using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using form_builder.Models;
using form_builder.Enum;
using form_builder.Validators;
using System.Threading.Tasks;
using System;
using form_builder.Helpers.PageHelpers;
using form_builder.Models.Elements;
using form_builder.Providers.SchemaProvider;
using Microsoft.Extensions.Logging;
using form_builder.Helpers.Session;

namespace form_builder.Controllers
{
    public class AddressController : Controller
    {
        private readonly IEnumerable<IElementValidator> _validators;

        private readonly ISchemaProvider _schemaProvider;

        private readonly IPageHelper _pageHelper;

        private readonly ISessionHelper _sessionHelper;

        private readonly ILogger<HomeController> _logger;

        public AddressController(ILogger<HomeController> logger, IEnumerable<IElementValidator> validators, ISchemaProvider schemaProvider, IPageHelper pageHelper, ISessionHelper sessionHelper)
        {
            _validators = validators;
            _schemaProvider = schemaProvider;
            _pageHelper = pageHelper;
            _logger = logger;
            _sessionHelper = sessionHelper;
        }

        [HttpGet]
        [Route("{form}/{path}/manual")]
        public async Task<IActionResult> AddressManual(string form, string path)
        {
            try
            {
                var sessionGuid = _sessionHelper.GetSessionGuid();

                if (sessionGuid == null)
                {
                    sessionGuid = Guid.NewGuid().ToString();
                    _sessionHelper.SetSessionGuid(sessionGuid);
                }

                var baseForm = await _schemaProvider.Get<FormSchema>(form);

                if (string.IsNullOrEmpty(path))
                {
                    path = baseForm.StartPageSlug;
                }

                var page = baseForm.GetPage(path);
                if (page == null)
                {
                    throw new ApplicationException($"AddressController: GetPage returned null for path: {path} of form: {form}, while performing Get");
                }
                
                var addressManualElememt = new AddressManual() { Properties = page.Elements[0].Properties, Type = EElementType.AddressManual };
              
                page.Elements[0] = addressManualElememt;
                var viewModel = await _pageHelper.GenerateHtml(page, new Dictionary<string, string>(), baseForm, sessionGuid);
                viewModel.AddressStatus = "Search";
                viewModel.FormName = baseForm.FormName;

                return View("Index",viewModel);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"AddressController: An exception has occured while attempting to return Address view Exception: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("{form}/{path}/manual")]
        public async Task<IActionResult> AddressManual(string form, string path, Dictionary<string, string[]> formData)
        {
            var baseForm = await _schemaProvider.Get<FormSchema>(form);
            var currentPage = baseForm.GetPage(path);
            var viewModel = NormaliseFormData(formData);            
            var sessionGuid = _sessionHelper.GetSessionGuid();

            var addressManualElememt = new AddressManual() { Properties = currentPage.Elements[0].Properties, Type = EElementType.AddressManual };
            addressManualElememt.SetAddressProperties(viewModel);

            currentPage.Elements[0] = addressManualElememt;

            if (currentPage == null)
            {
                throw new NullReferenceException($"Current page '{path}' object could not be found.");
            }

            currentPage.Validate(viewModel, _validators);
            if (!currentPage.IsValid)
            {
                
                var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, sessionGuid);
                formModel.Path = currentPage.PageSlug;
                formModel.FormName = baseForm.FormName;         
                return View("Index", formModel);
            }

            var behaviour = currentPage.GetNextPage(viewModel);
            _pageHelper.SaveAnswers(viewModel, sessionGuid);

            switch (behaviour.BehaviourType)
            {
                case EBehaviourType.GoToExternalPage:
                    return Redirect(behaviour.PageSlug);
                case EBehaviourType.GoToPage:
                    return RedirectToAction("Index", "Home", new
                    {
                        path = behaviour.PageSlug,
                        form = baseForm.BaseURL
                    });
                case EBehaviourType.SubmitForm:
                    return RedirectToAction("Submit", "Home", new
                    {
                        form = baseForm.BaseURL
                    });
                default:
                    throw new ApplicationException($"The provided behaviour type '{behaviour.BehaviourType}' is not valid");
            }
        }

        protected Dictionary<string, string> NormaliseFormData(Dictionary<string, string[]> formData)
        {

            var normaisedFormData = new Dictionary<string, string>();

            foreach (var item in formData)
            {
                if (item.Value.Length == 1)
                {
                    normaisedFormData.Add(item.Key, item.Value[0]);
                }
                else
                {
                    normaisedFormData.Add(item.Key, string.Join(", ", item.Value));
                }

            }

            return normaisedFormData;
        }
    }
}
