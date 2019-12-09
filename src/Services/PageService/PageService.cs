using System.Collections.Generic;
using form_builder.Models;
using form_builder.Validators;
using System.Threading.Tasks;
using form_builder.Helpers.PageHelpers;
using form_builder.Providers.SchemaProvider;
using Microsoft.Extensions.Logging;
using form_builder.Helpers.Session;
using System;
using form_builder.Services.PageService.Entities;
using System.Linq;
using form_builder.Enum;
using form_builder.Services.AddressService;
using form_builder.Services.StreetService;
using form_builder.Models.Elements;

namespace form_builder.Services.PageService
{
    public interface IPageService
    {
        Task<ProcessPageEntity> ProcessPage(string form, string path, Dictionary<string, string> viewModel, bool processManual = false);
    }

    public class PageService : IPageService
    {
        private readonly IEnumerable<IElementValidator> _validators;
        private readonly ISchemaProvider _schemaProvider;
        private readonly IPageHelper _pageHelper;
        private readonly ISessionHelper _sessionHelper;
        private readonly ILogger<PageService> _logger;
        private readonly IStreetService _streetService;
        private readonly IAddressService _addressService;

        public PageService(ILogger<PageService> logger, IEnumerable<IElementValidator> validators, ISchemaProvider schemaProvider, IPageHelper pageHelper, ISessionHelper sessionHelper, IAddressService addressService, IStreetService streetService)
        {
            _validators = validators;
            _schemaProvider = schemaProvider;
            _pageHelper = pageHelper;
            _sessionHelper = sessionHelper;
            _logger = logger;
            _streetService = streetService;
            _addressService = addressService;
        }

        public async Task<ProcessPageEntity> ProcessPage(string form, string path, Dictionary<string, string> viewModel, bool processManual)
        {
            var baseForm = await _schemaProvider.Get<FormSchema>(form);
            var currentPage = baseForm.GetPage(path);

            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (currentPage == null)
            {
                throw new NullReferenceException($"Current page '{path}' object could not be found.");
            }

            if (processManual)
            {
                var addressManualElememt = new AddressManual() { Properties = currentPage.Elements[0].Properties, Type = EElementType.AddressManual };                
                currentPage.Elements[0] = addressManualElememt;
            }

            currentPage.Validate(viewModel, _validators);

            if (currentPage.Elements.Any(_ => _.Type == EElementType.Address) && !processManual)
            {
                return await _addressService.ProcesssAddress(viewModel, currentPage, baseForm, sessionGuid, path);
            }

            if (currentPage.Elements.Any(_ => _.Type == EElementType.Street))
            {
                return await _streetService.ProcessStreet(viewModel, currentPage, baseForm, sessionGuid, path);
            }

            if (currentPage.IsValid)
            {
                _pageHelper.SaveAnswers(viewModel, sessionGuid);
            }
            else
            {
                var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, sessionGuid);
                formModel.Path = currentPage.PageSlug;
                formModel.FormName = baseForm.FormName;
                return new ProcessPageEntity
                {
                    Page = currentPage,
                    ViewModel = formModel
                };
            }

            return new ProcessPageEntity
            {
                Page = currentPage
            };
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
