using form_builder.Constants;
using form_builder.ContentFactory.PageFactory;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.Organisation;
using form_builder.Providers.StorageProvider;
using form_builder.Services.PageService.Entities;
using Newtonsoft.Json;

namespace form_builder.Services.OrganisationService;

public class OrganisationService : IOrganisationService
{
    private readonly IDistributedCacheWrapper _distributedCache;
    private readonly IPageHelper _pageHelper;
    private readonly IEnumerable<IOrganisationProvider> _organisationProviders;
    private readonly IPageFactory _pageFactory;

    public OrganisationService(IDistributedCacheWrapper distributedCache, IEnumerable<IOrganisationProvider> organisationProviders, IPageHelper pageHelper, IPageFactory pageFactory)
    {
        _distributedCache = distributedCache;
        _pageHelper = pageHelper;
        _organisationProviders = organisationProviders;
        _pageFactory = pageFactory;
    }

    public async Task<ProcessRequestEntity> ProcessOrganisation(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string cacheKey, string path)
    {
        viewModel.TryGetValue(LookUpConstants.SubPathViewModelKey, out var subPath);

        switch (subPath as string)
        {
            case LookUpConstants.Automatic:
                return await ProccessAutomaticOrganisation(viewModel, currentPage, baseForm, cacheKey, path);
            default:
                return await ProccessInitialOrganisation(viewModel, currentPage, baseForm, cacheKey, path);
        }
    }

    private async Task<ProcessRequestEntity> ProccessAutomaticOrganisation(
        Dictionary<string, dynamic> viewModel,
        Page currentPage,
        FormSchema baseForm,
        string cacheKey,
        string path)
    {
        var cachedAnswers = _distributedCache.GetString(cacheKey);
        var organisationElement = currentPage.Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.Organisation));
        var convertedAnswers = cachedAnswers is null
            ? new FormAnswers { Pages = new List<PageAnswers>() }
            : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

        var organisation = (string)convertedAnswers
            .Pages
            .FirstOrDefault(_ => _.PageSlug.Equals(path))
            .Answers
            .FirstOrDefault(_ => _.QuestionId.Equals($"{organisationElement.Properties.QuestionId}"))
            .Response;

        if (currentPage.IsValid && organisationElement.Properties.Optional && string.IsNullOrEmpty(organisation))
        {
            _pageHelper.SaveAnswers(viewModel, cacheKey, baseForm.BaseURL, null, currentPage.IsValid);

            return new ProcessRequestEntity
            {
                Page = currentPage
            };
        }

        if (!currentPage.IsValid)
        {
            var cachedSearchResults = convertedAnswers.FormData[$"{path}{LookUpConstants.SearchResultsKeyPostFix}"] as IEnumerable<object>;
            var model = await _pageFactory.Build(currentPage, viewModel, baseForm, cacheKey, convertedAnswers, cachedSearchResults.ToList());

            return new ProcessRequestEntity
            {
                Page = currentPage,
                ViewModel = model
            };
        }

        _pageHelper.SaveAnswers(viewModel, cacheKey, baseForm.BaseURL, null, currentPage.IsValid);

        return new ProcessRequestEntity
        {
            Page = currentPage
        };
    }

    private async Task<ProcessRequestEntity> ProccessInitialOrganisation(
        Dictionary<string, dynamic> viewModel,
        Page currentPage,
        FormSchema baseForm,
        string cacheKey,
        string path)
    {
        var cachedAnswers = _distributedCache.GetString(cacheKey);
        var organisationElement = currentPage.Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.Organisation));

        var convertedAnswers = cachedAnswers is null
            ? new FormAnswers { Pages = new List<PageAnswers>() }
            : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

        var organisation = (string)viewModel[$"{organisationElement.Properties.QuestionId}"];

        if (currentPage.IsValid && organisationElement.Properties.Optional && string.IsNullOrEmpty(organisation))
        {
            _pageHelper.SaveAnswers(viewModel, cacheKey, baseForm.BaseURL, null, currentPage.IsValid);

            return new ProcessRequestEntity
            {
                Page = currentPage
            };
        }

        if (!currentPage.IsValid)
        {
            var formModel = await _pageFactory.Build(currentPage, viewModel, baseForm, cacheKey, convertedAnswers);

            formModel.Path = currentPage.PageSlug;
            formModel.FormName = baseForm.FormName;
            formModel.PageTitle = currentPage.Title;
            formModel.HideBackButton = currentPage.HideBackButton;

            return new ProcessRequestEntity
            {
                Page = currentPage,
                ViewModel = formModel
            };
        }

        var foundOrganisationSearchTerm = convertedAnswers
            .Pages?.FirstOrDefault(_ => _.PageSlug.Equals(path))?
            .Answers?.FirstOrDefault(_ => _.QuestionId.Equals(organisationElement.Properties.QuestionId))?
            .Response;

        List<object> searchResults;
        if (organisation.Equals(foundOrganisationSearchTerm))
        {
            searchResults = (convertedAnswers.FormData[$"{path}{LookUpConstants.SearchResultsKeyPostFix}"] as IEnumerable<object>).ToList();
        }
        else
        {
            try
            {
                searchResults = (await _organisationProviders.Get(organisationElement.Properties.OrganisationProvider).SearchAsync(organisation)).ToList<object>();
            }
            catch (Exception e)
            {
                throw new ApplicationException($"OrganisationService.ProcessInitialOrganisation:: An exception has occurred while attempting to perform organisation lookup, Exception: {e.Message}");
            }

            _pageHelper.SaveAnswers(viewModel, cacheKey, baseForm.BaseURL, null, currentPage.IsValid);
            _pageHelper.SaveFormData($"{path}{LookUpConstants.SearchResultsKeyPostFix}", searchResults, cacheKey, baseForm.BaseURL);
        }

        return new ProcessRequestEntity
        {
            RedirectToAction = true,
            RedirectAction = "Index",
            RouteValues = new
            {
                form = baseForm.BaseURL,
                path,
                subPath = LookUpConstants.Automatic
            }
        };
    }
}