﻿using System.Collections.Generic;
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
using form_builder.ViewModels;
using form_builder.Providers.StorageProvider;
using Newtonsoft.Json;
using form_builder.Services.OrganisationService;

namespace form_builder.Services.PageService
{
    public interface IPageService
    {
        Task<ProcessPageEntity> ProcessPage(string form, string path, bool isAddressManual = false);
        Task<ProcessRequestEntity> ProcessRequest(string form, string path, Dictionary<string, string> viewModel, bool processManual = false);
        Task<FormBuilderViewModel> GetViewModel(Page page, FormSchema baseForm, string path, string sessionGuid);
        Behaviour GetBehaviour(ProcessRequestEntity currentPageResult);
    }

    public class PageService : IPageService
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly IEnumerable<IElementValidator> _validators;
        private readonly ISchemaProvider _schemaProvider;
        private readonly IPageHelper _pageHelper;
        private readonly ISessionHelper _sessionHelper;
        private readonly ILogger<PageService> _logger;
        private readonly IStreetService _streetService;
        private readonly IAddressService _addressService;
        private readonly IOrganisationService _organisationService;

        public PageService(ILogger<PageService> logger, IEnumerable<IElementValidator> validators, ISchemaProvider schemaProvider, IPageHelper pageHelper, ISessionHelper sessionHelper, IAddressService addressService, IStreetService streetService, IOrganisationService organisationService, IDistributedCacheWrapper distributedCache)
        {
            _validators = validators;
            _schemaProvider = schemaProvider;
            _pageHelper = pageHelper;
            _sessionHelper = sessionHelper;
            _logger = logger;
            _streetService = streetService;
            _addressService = addressService;
            _organisationService = organisationService;
            _distributedCache = distributedCache;
        }
        public async Task<ProcessPageEntity> ProcessPage(string form, string path, bool isAddressManual = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                _sessionHelper.RemoveSessionGuid();
            }

            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
            {
                sessionGuid = Guid.NewGuid().ToString();
                _sessionHelper.SetSessionGuid(sessionGuid);
            }

            var baseForm = await _schemaProvider.Get<FormSchema>(form);

            var formData = _distributedCache.GetString(sessionGuid);

            if (formData == null && path != baseForm.StartPageSlug)
            {
                return new ProcessPageEntity 
                {
                    ShouldRedirect = true,
                    TargetPage = baseForm.StartPageSlug
                };
            }

            if (string.IsNullOrEmpty(path))
            {
                return new ProcessPageEntity 
                {
                    ShouldRedirect = true,
                    TargetPage = baseForm.StartPageSlug
                };
            }

            if (formData != null && path == baseForm.StartPageSlug)
            {
                var convertedFormData = JsonConvert.DeserializeObject<FormAnswers>(formData);
                if (form != convertedFormData.FormName)
                    _distributedCache.Remove(sessionGuid);
            }

            var page = baseForm.GetPage(path);
            if (page == null)
            {
                throw new ApplicationException($"Requested path '{path}' object could not be found.");
            }

            baseForm.ValidateFormSchema(_pageHelper, form, path);

            if (isAddressManual)
            {
                var addressManualElememt = new AddressManual() { Properties = page.Elements[0].Properties, Type = EElementType.AddressManual };
                page.Elements[0] = addressManualElememt;
            }

            var viewModel = await GetViewModel(page, baseForm, path, sessionGuid);

            if (page.Elements.Any(_ => _.Type == EElementType.Street))
            {
                viewModel.StreetStatus = "Search";
                return new ProcessPageEntity
                {
                    ViewModel = viewModel,
                    ViewName = "../Street/Index"
                };
            }

            if (page.Elements.Any(_ => _.Type == EElementType.Address))
            {
                viewModel.AddressStatus = "Search";
                return new ProcessPageEntity
                {
                    ViewModel = viewModel,
                    ViewName = "../Address/Index"
                };
            }

            if (page.Elements.Any(_ => _.Type == EElementType.Organisation))
            {
                viewModel.OrganisationStatus = "Search";
                return new ProcessPageEntity
                {
                    ViewModel = viewModel,
                    ViewName = "../Organisation/Index"
                };
            }

            return new ProcessPageEntity
            {
                ViewModel = viewModel
            };
        }

        public async Task<ProcessRequestEntity> ProcessRequest(string form, string path, Dictionary<string, string> viewModel, bool processManual)
        {
            var baseForm = await _schemaProvider.Get<FormSchema>(form);
            var currentPage = baseForm.GetPage(path);

            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (sessionGuid == null)
            {
                throw new NullReferenceException($"Session guid null.");
            }

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

            if (currentPage.Elements.Any(_ => _.Type == EElementType.Organisation))
            {
                return await _organisationService.ProcesssOrganisation(viewModel, currentPage, baseForm, sessionGuid, path);
            }

            _pageHelper.SaveAnswers(viewModel, sessionGuid, baseForm.BaseURL);

            if (!currentPage.IsValid)
            {
                var formModel = await _pageHelper.GenerateHtml(currentPage, viewModel, baseForm, sessionGuid);
                formModel.Path = currentPage.PageSlug;
                formModel.FormName = baseForm.FormName;
                formModel.PageTitle = currentPage.Title;
                return new ProcessRequestEntity
                {
                    Page = currentPage,
                    ViewModel = formModel
                };
            }

            return new ProcessRequestEntity
            {
                Page = currentPage
            };
        }

        public async Task<FormBuilderViewModel> GetViewModel(Page page, FormSchema baseForm, string path, string sessionGuid)
        {
            var viewModel = await _pageHelper.GenerateHtml(page, new Dictionary<string, string>(), baseForm, sessionGuid);
            viewModel.FormName = baseForm.FormName;
            viewModel.PageTitle = page.Title;
            viewModel.Path = path;

            return viewModel;
        }
        public Behaviour GetBehaviour(ProcessRequestEntity currentPageResult)
        {
            Dictionary<string, string> answers = new Dictionary<string, string>();

            var sessionGuid = _sessionHelper.GetSessionGuid();
            var cachedAnswers = _distributedCache.GetString(sessionGuid);
            var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

            convertedAnswers.Pages
                .SelectMany(_ => _.Answers)
                .ToList()
                .ForEach(x => answers.Add(x.QuestionId, x.Response));

            return currentPageResult.Page.GetNextPage(answers);
        }
    }
}
