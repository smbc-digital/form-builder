namespace form_builder.Services.StreetService;

public class StreetService(IDistributedCacheWrapper distributedCache,
    IEnumerable<IStreetProvider> streetProviders,
    IPageHelper pageHelper,
    IPageFactory pageFactory)
    : IStreetService
{
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
        var cachedAnswers = distributedCache.GetString(cacheKey);
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
            pageHelper.SaveAnswers(viewModel, cacheKey, baseForm.BaseURL, null, currentPage.IsValid);
            return new ProcessRequestEntity
            {
                Page = currentPage
            };
        }

        if (!currentPage.IsValid)
        {
            var cachedSearchResults = convertedAnswers.FormData[$"{path}{LookUpConstants.SearchResultsKeyPostFix}"] as IEnumerable<object>;

            var model = await pageFactory.Build(currentPage, viewModel, baseForm, cacheKey, convertedAnswers, cachedSearchResults.ToList());

            return new ProcessRequestEntity
            {
                Page = currentPage,
                ViewModel = model
            };
        }

        pageHelper.SaveAnswers(viewModel, cacheKey, baseForm.BaseURL, null, currentPage.IsValid);

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
        var cachedAnswers = distributedCache.GetString(cacheKey);
        var streetElement = currentPage.Elements.Where(_ => _.Type.Equals(EElementType.Street)).FirstOrDefault();

        var convertedAnswers = cachedAnswers is null
            ? new FormAnswers { Pages = new List<PageAnswers>() }
            : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

        var street = (string)viewModel[streetElement.Properties.QuestionId];

        if (currentPage.IsValid && streetElement.Properties.Optional && string.IsNullOrEmpty(street))
        {
            pageHelper.SaveAnswers(viewModel, cacheKey, baseForm.BaseURL, null, currentPage.IsValid);
            return new ProcessRequestEntity
            {
                Page = currentPage
            };
        }

        if (!currentPage.IsValid)
        {
            var formModel = await pageFactory.Build(currentPage, viewModel, baseForm, cacheKey, convertedAnswers);

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
                searchResults = (await streetProviders.Get(streetElement.Properties.StreetProvider).SearchAsync(street)).ToList<object>();
            }
            catch (Exception e)
            {
                throw new ApplicationException($"StreetService::ProcessInitialStreet: An exception has occurred while attempting to perform street lookup on Provider '{streetElement.Properties.StreetProvider}' with searchterm '{street}' Exception: {e.Message}");
            }

            pageHelper.SaveAnswers(viewModel, cacheKey, baseForm.BaseURL, null, currentPage.IsValid);
            pageHelper.SaveFormData($"{path}{LookUpConstants.SearchResultsKeyPostFix}", searchResults, cacheKey, baseForm.BaseURL);
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