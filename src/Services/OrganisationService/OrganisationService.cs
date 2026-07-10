namespace form_builder.Services.OrganisationService;

public class OrganisationService(IDistributedCacheWrapper distributedCache,
    IEnumerable<IOrganisationProvider> organisationProviders,
    IPageHelper pageHelper,
    IPageFactory pageFactory)
    : IOrganisationService
{
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
        var cachedAnswers = distributedCache.GetString(cacheKey);
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

    private async Task<ProcessRequestEntity> ProccessInitialOrganisation(
        Dictionary<string, dynamic> viewModel,
        Page currentPage,
        FormSchema baseForm,
        string cacheKey,
        string path)
    {
        var cachedAnswers = distributedCache.GetString(cacheKey);
        var organisationElement = currentPage.Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.Organisation));

        var convertedAnswers = cachedAnswers is null
            ? new FormAnswers { Pages = new List<PageAnswers>() }
            : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

        var organisation = (string)viewModel[$"{organisationElement.Properties.QuestionId}"];

        if (currentPage.IsValid && organisationElement.Properties.Optional && string.IsNullOrEmpty(organisation))
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
                searchResults = (await organisationProviders.Get(organisationElement.Properties.OrganisationProvider).SearchAsync(organisation)).ToList<object>();
            }
            catch (Exception e)
            {
                throw new ApplicationException($"OrganisationService.ProcessInitialOrganisation:: An exception has occurred while attempting to perform organisation lookup, Exception: {e.Message}");
            }

            pageHelper.SaveAnswers(viewModel, cacheKey, baseForm.BaseURL, null, currentPage.IsValid);
            pageHelper.SaveFormData($"{path}{LookUpConstants.SearchResultsKeyPostFix}", searchResults, cacheKey, baseForm.BaseURL);
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