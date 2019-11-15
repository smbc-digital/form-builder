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

            if (currentPage == null)
            {
                return RedirectToAction("Error");
            }

            var viewModel = NormaliseFormData(formData);
            var guid = Guid.Parse(viewModel["Guid"]);
            var cachedAnswers = _distributedCache.GetString(guid.ToString());
            var convertedAnswers = cachedAnswers == null
                ? new FormAnswers { Pages = new List<PageAnswers>() }
                : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);
            
            var journey = viewModel["AddressStatus"];

            var addressElement = currentPage.Elements.Where(_ => _.Type == EElementType.Address).FirstOrDefault();
            var provider = _addressProviders.ToList()
                    .Where(_ => _.ProviderName == addressElement.Properties.AddressProvider)
                    .FirstOrDefault();

            var postcode = journey == "Select" 
                ? convertedAnswers.Pages.FirstOrDefault(_ => _.PageUrl == path).Answers.FirstOrDefault(_ => _.QuestionId == $"{addressElement.Properties.QuestionId}-postcode").Response 
                : viewModel[$"{addressElement.Properties.QuestionId}-postcode"];

            var addressResults = await provider.SearchAsync(postcode);

            currentPage.Validate(viewModel, _validators);
            if (!currentPage.IsValid)
            {
                var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, addressResults);
                formModel.Path = currentPage.PageURL;
                formModel.Guid = guid;
                return View(formModel);
            }

            var behaviour = currentPage.GetNextPage(viewModel);
            _pageHelper.SaveAnswers(viewModel);

            switch (journey)
            {
                case "Search":
                    try
                    {
                        var adddressViewModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, addressResults);
                        adddressViewModel.AddressStatus = "Select";

                        return View(adddressViewModel);
                    }
                    catch (Exception e)
                    {
                        return RedirectToAction("Error");
                    };
                case "Select":
                    //var adddressViewModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, result);

                    //return View(adddressViewModel);
                    break;
                case "Manual":
                    break;
                default:
                    return RedirectToAction("Error");
                    break;
            }

            return RedirectToAction("Error");
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
