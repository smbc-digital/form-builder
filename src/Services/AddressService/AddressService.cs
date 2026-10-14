namespace form_builder.Services.AddressService;

public class AddressService(IDistributedCacheWrapper distributedCache,
    IPageHelper pageHelper,
    IEnumerable<IAddressProvider> addressProviders,
    IPageFactory pageFactory,
    ILogger<AddressService> logger)
    : IAddressService
{
    public async Task<ProcessRequestEntity> ProcessAddress(
        Dictionary<string, dynamic> viewModel,
        Page currentPage,
        FormSchema baseForm,
        string guid,
        string path)
    {
        viewModel.TryGetValue(LookUpConstants.SubPathViewModelKey, out var subPath);

        switch (subPath)
        {
            case LookUpConstants.Manual:
                return await ProcessManualAddress(viewModel, currentPage, baseForm, guid, path);
            case LookUpConstants.Automatic:
                return await ProcessAutomaticAddress(viewModel, currentPage, baseForm, guid, path);
            default:
                return await ProcessSearchAddress(viewModel, currentPage, baseForm, guid, path);
        }
    }

    private async Task<ProcessRequestEntity> ProcessManualAddress(
        Dictionary<string, dynamic> viewModel,
        Page currentPage,
        FormSchema baseForm,
        string guid,
        string path)
    {
        pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);

        if (!currentPage.IsValid)
        {
            var cachedAnswers = distributedCache.GetString(guid);

            var convertedAnswers = cachedAnswers is null
                ? new FormAnswers { Pages = new List<PageAnswers>() }
                : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

            var cachedSearchResults = convertedAnswers.FormData[$"{path}{LookUpConstants.SearchResultsKeyPostFix}"] as IEnumerable<object>;

            var model = await pageFactory.Build(currentPage, viewModel, baseForm, guid, convertedAnswers, cachedSearchResults.ToList());

            return new ProcessRequestEntity
            {
                Page = currentPage,
                ViewModel = model
            };
        }

        return new ProcessRequestEntity
        {
            Page = currentPage
        };
    }

    private async Task<ProcessRequestEntity> ProcessAutomaticAddress(
        Dictionary<string, dynamic> viewModel,
        Page currentPage,
        FormSchema baseForm,
        string guid,
        string path)
    {
        var cachedAnswers = distributedCache.GetString(guid);

        var convertedAnswers = cachedAnswers is null
            ? new FormAnswers { Pages = new List<PageAnswers>() }
            : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

        var addressElement = currentPage.Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.Address));

        var postcode = (string)convertedAnswers
            .Pages
            .FirstOrDefault(_ => _.PageSlug.Equals(path))
            .Answers
            .FirstOrDefault(_ => _.QuestionId.Equals($"{addressElement.Properties.QuestionId}{AddressConstants.SEARCH_SUFFIX}"))
            .Response;

        var address = (string)viewModel[$"{addressElement.Properties.QuestionId}{AddressConstants.SELECT_SUFFIX}"];

        if (currentPage.IsValid && addressElement.Properties.Optional && string.IsNullOrEmpty(postcode))
        {
            pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);
            return new ProcessRequestEntity
            {
                Page = currentPage
            };
        }

        if (currentPage.IsValid && addressElement.Properties.Optional && string.IsNullOrEmpty(address) && !string.IsNullOrEmpty(postcode))
        {
            pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);
            return new ProcessRequestEntity
            {
                Page = currentPage
            };
        }

        if (!currentPage.IsValid)
        {
            var cachedSearchResults = convertedAnswers.FormData[$"{path}{LookUpConstants.SearchResultsKeyPostFix}"] as IEnumerable<object>;

            var model = await pageFactory.Build(currentPage, viewModel, baseForm, guid, convertedAnswers, cachedSearchResults.ToList());

            return new ProcessRequestEntity
            {
                Page = currentPage,
                ViewModel = model
            };
        }

        pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);

        return new ProcessRequestEntity
        {
            Page = currentPage
        };
    }

    private async Task<ProcessRequestEntity> ProcessSearchAddress(
        Dictionary<string, dynamic> viewModel,
        Page currentPage,
        FormSchema baseForm,
        string guid,
        string path)
    {
        var cachedAnswers = distributedCache.GetString(guid);

        var convertedAnswers = cachedAnswers is null
            ? new FormAnswers { Pages = new List<PageAnswers>() }
            : JsonConvert.DeserializeObject<FormAnswers>(cachedAnswers);

        var addressElement = currentPage.Elements.FirstOrDefault(_ => _.Type.Equals(EElementType.Address));

        if (!currentPage.IsValid)
        {
            var formModel = await pageFactory.Build(currentPage, viewModel, baseForm, guid, convertedAnswers, null);

            return new ProcessRequestEntity
            {
                Page = currentPage,
                ViewModel = formModel
            };
        }

        var postcode = (string)viewModel[$"{addressElement.Properties.QuestionId}{AddressConstants.SEARCH_SUFFIX}"];

        if (addressElement.Properties.Optional && string.IsNullOrEmpty(postcode))
        {
            pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);
            return new ProcessRequestEntity
            {
                Page = currentPage
            };
        }

        var foundPostCode = convertedAnswers
            .Pages?.FirstOrDefault(_ => _.PageSlug.Equals(path))?
            .Answers?.FirstOrDefault(_ => _.QuestionId.Equals($"{addressElement.Properties.QuestionId}{AddressConstants.SEARCH_SUFFIX}"))?
            .Response;

        List<object> addressResults = new();
        IAddressProvider addressProv;
        if (postcode.Equals(foundPostCode))
        {
            addressResults = (convertedAnswers.FormData[$"{path}{LookUpConstants.SearchResultsKeyPostFix}"] as IEnumerable<object>).ToList();
        }
        else
        {
            try
            {
                bool fullUKPostcode = addressElement.Properties.FullUKPostcode;
                if (fullUKPostcode)
                    postcode = postcode + ":full";

                addressProv = addressProviders.Get(addressElement.Properties.AddressProvider);

                if (addressProv is null)
                {
                    logger.LogWarning($"{nameof(AddressService)}::{nameof(ProcessSearchAddress)}: Address Provider could not be set for {addressElement.Properties.AddressProvider}");
                    throw new ApplicationException($"AddressService::ProcessSearchAddress, An exception has occurred while attempting to get address provider = '{addressElement.Properties.AddressProvider}'");
                }

                logger.LogWarning($"{nameof(AddressService)}::{nameof(ProcessSearchAddress)}: Address Provider set successfully for {addressElement.Properties.AddressProvider}");
            }
            catch (Exception exception)
            {
                logger.LogWarning($"{nameof(AddressService)}::{nameof(ProcessSearchAddress)}: Unexpected exception getting address provider {addressElement.Properties.AddressProvider}");
                throw exception;
            }

            try
            {
                logger.LogWarning($"{nameof(AddressService)}::{nameof(ProcessSearchAddress)}: Starting search async {postcode}");
                addressResults = new List<object>(await addressProv.SearchAsync(postcode));

            }
            catch (Exception exception)
            {
                logger.LogWarning($"{nameof(AddressService)}::{nameof(ProcessSearchAddress)}: Unexpected error occurred searching for {postcode}");
                throw exception;
            }

            pageHelper.SaveAnswers(viewModel, guid, baseForm.BaseURL, null, currentPage.IsValid);
            pageHelper.SaveFormData($"{path}{LookUpConstants.SearchResultsKeyPostFix}", addressResults, guid, baseForm.BaseURL);
        }

        if (!addressResults.Any() && !addressElement.Properties.DisableManualAddress)
        {
            return new ProcessRequestEntity
            {
                RedirectToAction = true,
                RedirectAction = "Index",
                RouteValues = new
                {
                    form = baseForm.BaseURL,
                    path,
                    subPath = LookUpConstants.Manual
                }
            };
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