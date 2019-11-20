using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using form_builder.Models;
using form_builder.Enum;
using form_builder.Validators;
using System.Threading.Tasks;
using System;
using StockportGovUK.AspNetCore.Gateways;
using form_builder.Helpers.PageHelpers;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using Microsoft.Extensions.Logging;
using form_builder.Providers.Address;
using System.Linq;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Models.Addresses;

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
                viewModel.Guid = guid;

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
            var baseForm = await _schemaProvider.Get<FormSchema>(form);
            var currentPage = baseForm.GetPage(path);

            if (currentPage == null)
            {
                return RedirectToAction("Error");
            }

            var viewModel = NormaliseFormData(formData);
            var guid = Guid.Parse(viewModel["Guid"]);

            var journey = viewModel["AddressStatus"];
            var addressResults = new List<AddressSearchResult>();

            currentPage.Validate(viewModel, _validators);

            if ((!currentPage.IsValid && journey == "Select") || (currentPage.IsValid && journey == "Search"))
            {
                var cachedAnswers = _distributedCache.GetString(guid.ToString());
                var convertedAnswers = cachedAnswers == null
                    ? new FormAnswers { Pages = new List<PageAnswers>() }
                    : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

                var addressElement = currentPage.Elements.Where(_ => _.Type == EElementType.Address).FirstOrDefault();
                var provider = _addressProviders.ToList()
                    .Where(_ => _.ProviderName == addressElement.Properties.AddressProvider)
                    .FirstOrDefault();

                if (provider == null)
                {
                    return RedirectToAction("Error", "Home", new
                    {
                        form = baseForm.BaseURL,
                        ex = $"No address provider configure for {addressElement.Properties.AddressProvider}"
                    });
                }

                var postcode = journey == "Select"
                    ? convertedAnswers.Pages.FirstOrDefault(_ => _.PageUrl == path).Answers.FirstOrDefault(_ => _.QuestionId == $"{addressElement.Properties.QuestionId}-postcode").Response
                    : viewModel[$"{addressElement.Properties.QuestionId}-postcode"];

                try
                {
                    var result = await provider.SearchAsync(postcode);
                    addressResults = result.ToList();
                }
                catch (Exception e)
                {
                    _logger.LogError($"AddressController: An exception has occured while attempting to perform postcode lookup, Exception: {e.Message}");
                    return RedirectToAction("Error", "Home", new { form = baseForm.BaseURL, });
                }
            }

            if (!currentPage.IsValid)
            {
                var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, addressResults);
                formModel.Path = currentPage.PageURL;
                formModel.Guid = guid;
                formModel.AddressStatus = journey;
                return View(formModel);
            }

            _pageHelper.SaveAnswers(viewModel);

            switch (journey)
            {
                case "Search":
                    try
                    {
                        var adddressViewModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, addressResults);
                        adddressViewModel.AddressStatus = "Select";
                        adddressViewModel.Guid = guid;
                        return View(adddressViewModel);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"AddressController: An exception has occured while attempting to generate Html, Exception: {e.Message}");
                        return RedirectToAction("Error", "Home", new { form = baseForm.BaseURL, });
                    };
                case "Select":
                    var behaviour = currentPage.GetNextPage(viewModel);
                    switch (behaviour.BehaviourType)
                    {
                        case EBehaviourType.GoToExternalPage:
                            return Redirect(behaviour.pageURL);
                        case EBehaviourType.GoToPage:
                            return RedirectToAction("Index", "Home", new
                            {
                                path = behaviour.pageURL,
                                guid,
                                form = baseForm.BaseURL
                            });
                        case EBehaviourType.SubmitForm:
                            return RedirectToAction("Submit", "Home", new
                            {
                                form = baseForm.BaseURL,
                                guid
                            });
                        default:
                            return RedirectToAction("Error", "Home", new { form = baseForm.BaseURL, });
                    }
                case "Manual":
                    break;
                default:
                    return RedirectToAction("Error", "Home", new { form = baseForm.BaseURL, });
            }

            return RedirectToAction("Error", "Home", new { form = baseForm.BaseURL, });
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
