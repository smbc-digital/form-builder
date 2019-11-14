using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using form_builder.Models;
using form_builder.ViewModels;
using Newtonsoft.Json;
using form_builder.Enum;
using form_builder.Validators;
using System.Threading.Tasks;
using System;
using StockportGovUK.AspNetCore.Gateways;
using form_builder.Helpers.PageHelpers;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using Microsoft.Extensions.Logging;
using System.Net;
using form_builder.Providers.Address;
using System.Linq;

namespace form_builder.Controllers
{
    public class AddressController : Controller
    {
        private readonly IDistributedCacheWrapper _distributedCache;

        private readonly IEnumerable<IElementValidator> _validators;

        private readonly ISchemaProvider _schemaProvider;

        private readonly IGateway _gateway;

        private readonly IPageHelper _pageHelper;

        private readonly ILogger<HomeController> _logger;

        private readonly IEnumerable<IAddressProvider> _addressProviders;

        public AddressController(ILogger<HomeController> logger, IDistributedCacheWrapper distributedCache, IEnumerable<IElementValidator> validators, ISchemaProvider schemaProvider, IGateway gateway, IPageHelper pageHelper, IEnumerable<IAddressProvider> addressProviders)
        {
            _distributedCache = distributedCache;
            _validators = validators;
            _schemaProvider = schemaProvider;
            _gateway = gateway;
            _pageHelper = pageHelper;
            _logger = logger;
            _addressProviders = addressProviders;
        }

        [HttpGet]
        [Route("{form}/{path}/address")]
        public async Task<IActionResult> Index(string form, string path, [FromQuery] Guid guid)
        {
            try
            {
                var baseForm = await _schemaProvider.Get<FormSchema>(form);

                if (Guid.Empty == guid)
                {
                    guid = Guid.NewGuid();
                }

                if (string.IsNullOrEmpty(path))
                {
                    path = baseForm.StartPage;
                }

                var page = baseForm.GetPage(path);
                if (page == null)
                {
                    return RedirectToAction("Error");
                }

                var viewModel = await _pageHelper.GenerateHtml(page, new Dictionary<string, string>(), baseForm);
                viewModel.AddressStatus = "Search";

                return View(viewModel);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Home", new { ex = ex.Message });
            }
        }

        [HttpPost]
        [Route("{form}/{path}/address")]
        public async Task<IActionResult> Index(string form, string path, Dictionary<string, string[]> formData)
        {
            // Search
            // Selection

            var baseForm = await _schemaProvider.Get<FormSchema>(form);
            var currentPage = baseForm.GetPage(path);
            var viewModel = NormaliseFormData(formData);

            var journey = viewModel["AddressStatus"];

            var guid = Guid.Parse(viewModel["Guid"]);

            if (currentPage == null)
            {
                return RedirectToAction("Error");
            }

            currentPage.Validate(viewModel, _validators);
            if (!currentPage.IsValid)
            {
                var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm);
                formModel.Path = currentPage.PageURL;
                formModel.Guid = guid;
                return View(formModel);
            }

            var behaviour = currentPage.GetNextPage(viewModel);
            _pageHelper.SaveAnswers(viewModel);

            switch (journey)
            {
                case "Search":
                    var addressElement = currentPage.Elements.Where(_ => _.Type == EElementType.Address).FirstOrDefault();
                    try
                    {

                        var provider = _addressProviders.ToList()
                            .Where(_ => _.ProviderName == addressElement.Properties.AddressProvider)
                            .FirstOrDefault();

                        var result = await provider.SearchAsync(viewModel[addressElement.Properties.QuestionId]);

                        var adddressViewModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, result);

                        return View(adddressViewModel);

                    }
                    catch (Exception  e)
                    {
                        return RedirectToAction("Error");
                    }
                    break;
                case "Select":
                    break;
                case "Manual":
                    break;
                default:
                    break;
            }
                
            switch (behaviour.BehaviourType)
            {
                case EBehaviourType.GoToExternalPage:
                    return Redirect(behaviour.pageURL);
                case EBehaviourType.GoToPage:
                    return RedirectToAction("Index", "Address", new
                    {
                        path = behaviour.pageURL,
                        guid,
                        form
                    });
                case EBehaviourType.SubmitForm:
                    return RedirectToAction("Submit", "Home", new
                    {
                        form = baseForm.BaseURL,
                        guid
                    });
                default:
                    return RedirectToAction("Error");
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
