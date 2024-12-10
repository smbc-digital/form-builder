using form_builder.Constants;
using form_builder.ContentFactory.PageFactory;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.PageHelpers;
using form_builder.Models;
using form_builder.Providers.StorageProvider;
using form_builder.Providers.Street;
using form_builder.Services.PageService.Entities;
using Newtonsoft.Json;

namespace form_builder.Services.StreetService;

public class StreetService : IStreetService
{
    private readonly IDistributedCacheWrapper _distributedCache;
    private readonly IPageHelper _pageHelper;
    private readonly IEnumerable<IStreetProvider> _streetProviders;
    private readonly IPageFactory _pageFactory;


    public StreetService(
        IDistributedCacheWrapper distributedCache,
        IEnumerable<IStreetProvider> streetProviders,
        IPageHelper pageHelper,
        IPageFactory pageFactory)
    {
        _distributedCache = distributedCache;
        _pageHelper = pageHelper;
        _streetProviders = streetProviders;
        _pageFactory = pageFactory;
    }

    public async Task<ProcessRequestEntity> ProcessStreet(Dictionary<string, dynamic> viewModel, Page currentPage, FormSchema baseForm, string cacheKey, string path)
    {
        viewModel.TryGetValue(LookUpConstants.SubPathViewModelKey, out var subPath);

        switch (subPath as string)
        {
            case LookUpConstants.Automatic:
                return await ProcessAutomaticStreet(viewModel, currentPage, baseForm, cacheKey, path);
            default:
                return await ProcessInitialStreet(viewModel, currentPage, baseForm, cacheKey, path);
        }
    }

    private async Task<ProcessRequestEntity> ProcessAutomaticStreet(
        Dictionary<string, dynamic> viewModel,
        Page currentPage,
        FormSchema baseForm,
        string cacheKey,
        string path)
    {
        var cachedAnswers = _distributedCache.GetString(cacheKey);
        var streetElement = currentPage.Elements.Where(_ => _.Type.Equals(EElementType.Street)).FirstOrDefault();
        var convertedAnswers = cachedAnswers is null
            ? new FormAnswers { Pages = new List<PageAnswers>() }
            : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

        var street = (string)convertedAnswers
            .Pages
            .FirstOrDefault(_ => _.PageSlug.Equals(path))
            .Answers
            .FirstOrDefault(_ => _.QuestionId.Equals($"{streetElement.Properties.QuestionId}"))
            .Response;

        if (currentPage.IsValid && streetElement.Properties.Optional && string.IsNullOrEmpty(street))
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

    private async Task<ProcessRequestEntity> ProcessInitialStreet(
        Dictionary<string, dynamic> viewModel,
        Page currentPage,
        FormSchema baseForm,
        string cacheKey,
        string path)
    {
        var cachedAnswers = _distributedCache.GetString(cacheKey);
        var streetElement = currentPage.Elements.Where(_ => _.Type.Equals(EElementType.Street)).FirstOrDefault();

        var convertedAnswers = cachedAnswers is null
            ? new FormAnswers { Pages = new List<PageAnswers>() }
            : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

        var street = (string)viewModel[streetElement.Properties.QuestionId];

        if (currentPage.IsValid && streetElement.Properties.Optional && string.IsNullOrEmpty(street))
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

            return new ProcessRequestEntity
            {
                Page = currentPage,
                ViewModel = formModel
            };
        }

        var foundStreet = convertedAnswers
            .Pages?.FirstOrDefault(_ => _.PageSlug.Equals(path))?
            .Answers?.FirstOrDefault(_ => _.QuestionId.Equals(streetElement.Properties.QuestionId))?
            .Response;

        List<object> searchResults;
        if (street.Equals(foundStreet))
        {
            searchResults = (convertedAnswers.FormData[$"{path}{LookUpConstants.SearchResultsKeyPostFix}"] as IEnumerable<object>).ToList();
        }
        else
        {
            try
            {
                searchResults = (await _streetProviders.Get(streetElement.Properties.StreetProvider).SearchAsync(street)).ToList<object>();
            }
            catch (Exception e)
            {
                throw new ApplicationException($"StreetService::ProcessInitialStreet: An exception has occurred while attempting to perform street lookup on Provider '{streetElement.Properties.StreetProvider}' with searchterm '{street}' Exception: {e.Message}");
            }

            _pageHelper.SaveAnswers(viewModel, cacheKey, baseForm.BaseURL, null, currentPage.IsValid);
            _pageHelper.SaveFormData($"{path}{LookUpConstants.SearchResultsKeyPostFix}", searchResults, cacheKey, baseForm.BaseURL);
        }

        try
        {
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
        catch (Exception e)
        {
            throw new ApplicationException($"PageHelper.ProcessInitialStreet: An exception has occured while attempting to generate Html, Exception: {e.Message}");
        }
    }
}