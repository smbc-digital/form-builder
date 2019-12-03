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
using form_builder.Helpers.Session;

namespace form_builder.Controllers
{
    public class AddressController : Controller
    {
        private readonly IDistributedCacheWrapper _distributedCache;

        private readonly IEnumerable<IElementValidator> _validators;

        private readonly ISchemaProvider _schemaProvider;

        private readonly IGateway _gateway;

        private readonly IPageHelper _pageHelper;

        private readonly ISessionHelper _sessionHelper;

        private readonly ILogger<HomeController> _logger;

        private readonly IEnumerable<IAddressProvider> _addressProviders;

        public AddressController(ILogger<HomeController> logger, IDistributedCacheWrapper distributedCache, IEnumerable<IElementValidator> validators, ISchemaProvider schemaProvider, IGateway gateway, IPageHelper pageHelper, IEnumerable<IAddressProvider> addressProviders, ISessionHelper sessionHelper)
        {
            _distributedCache = distributedCache;
            _validators = validators;
            _schemaProvider = schemaProvider;
            _gateway = gateway;
            _pageHelper = pageHelper;
            _logger = logger;
            _sessionHelper = sessionHelper;
            _addressProviders = addressProviders;
        }

        [HttpGet]
        [Route("{form}/{path}/address")]
        public async Task<IActionResult> Index(string form, string path)
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

                var viewModel = await _pageHelper.GenerateHtml(page, new Dictionary<string, string>(), baseForm, sessionGuid);
                viewModel.AddressStatus = "Search";
                viewModel.FormName = baseForm.FormName;

                return View(viewModel);
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"AddressController: An exception has occured while attempting to return Address view Exception: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("{form}/{path}/address-manual")]
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
                page.Elements[0].Type = EElementType.AddressManual;
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
        [Route("{form}/{path}/address-manual")]
        public async Task<IActionResult> AddressManual(string form, string path, Dictionary<string, string[]> formData)
        {
            var baseForm = await _schemaProvider.Get<FormSchema>(form);
            var currentPage = baseForm.GetPage(path);
            var viewModel = NormaliseFormData(formData);

            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (currentPage == null)
            {
                throw new NullReferenceException($"Current page '{path}' object could not be found.");
            }

            currentPage.Validate(viewModel, _validators);
            if (!currentPage.IsValid)
            {
                currentPage.Elements[0].Type = EElementType.AddressManual;
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
                    return RedirectToAction("Index", new
                    {
                        path = behaviour.PageSlug
                    });               
                default:
                    throw new ApplicationException($"The provided behaviour type '{behaviour.BehaviourType}' is not valid");
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
                throw new ApplicationException($"AddressController: GetPage returned null for path: {path} for form: {form}, while performing Post");
            }

            var viewModel = NormaliseFormData(formData);
            var guid = _sessionHelper.GetSessionGuid();

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
                    throw new ApplicationException($"No address provider configure for {addressElement.Properties.AddressProvider}");
                }

                var postcode = journey == "Select"
                    ? convertedAnswers.Pages.FirstOrDefault(_ => _.PageSlug == path).Answers.FirstOrDefault(_ => _.QuestionId == $"{addressElement.Properties.QuestionId}-postcode").Response
                    : viewModel[$"{addressElement.Properties.QuestionId}-postcode"];

                try
                {
                    var result = await provider.SearchAsync(postcode);
                    addressResults = result.ToList();
                }
                catch (Exception e)
                {
                    throw new ApplicationException($"AddressController: An exception has occured while attempting to perform postcode lookup, Exception: {e.Message}");
                }
            }

            if (!currentPage.IsValid)
            {
                var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, guid, addressResults);
                formModel.Path = currentPage.PageSlug;
                formModel.AddressStatus = journey;
                formModel.FormName = baseForm.FormName;

                return View(formModel);
            }

            _pageHelper.SaveAnswers(viewModel, guid);

            switch (journey)
            {
                case "Search":
                    try
                    {
                        var adddressViewModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, guid, addressResults);
                        adddressViewModel.AddressStatus = "Select";
                        adddressViewModel.FormName = baseForm.FormName;

                        return View(adddressViewModel);
                    }
                    catch (Exception e)
                    {
                        throw new ApplicationException($"AddressController: An exception has occured while attempting to generate Html, Exception: {e.Message}");
                    };
                case "Select":
                    var behaviour = currentPage.GetNextPage(viewModel);
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
                            throw new ApplicationException($"AddressController: Unknown behaviour type");
                    }
                case "Manual":
                    break;
                default:
                    throw new ApplicationException($"AddressController: Unknown journey type");
            }

            throw new ApplicationException($"AddressController: A generic error has occured");
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
