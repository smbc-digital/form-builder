namespace form_builder.Factories.Transform.UserSchema;

public class DynamicLookupPageTransformFactory(IActionHelper actionHelper,
    IEnumerable<ILookupProvider> lookupProviders,
    IWebHostEnvironment environment)
    : IUserPageTransformFactory
{
    public async Task<Page> Transform(Page page, FormAnswers convertedAnswers)
    {
        if (page.HasDynamicLookupElements)
        {
            foreach (var element in page.Elements.Where(_ => (_.Type.Equals(EElementType.Radio) || _.Type.Equals(EElementType.Checkbox) || _.Type.Equals(EElementType.Select)) &&
                                                             !string.IsNullOrEmpty(_.Lookup) && _.Lookup.Equals(LookUpConstants.Dynamic)))
            {
                await AddDynamicOptions(element, convertedAnswers);
            }
        }

        return page;
    }

    private async Task AddDynamicOptions(IElement element, FormAnswers convertedAnswers)
    {
        LookupSource submitDetails = element.Properties.LookupSources
            .SingleOrDefault(x => x.EnvironmentName
                .Equals(environment.EnvironmentName, StringComparison.OrdinalIgnoreCase));

        if (submitDetails is null)
            throw new Exception("DynamicLookupPageTransformFactory::AddDynamicOptions, No Environment specific details found");

        RequestEntity request = actionHelper.GenerateUrl(submitDetails.URL, convertedAnswers);

        if (string.IsNullOrEmpty(submitDetails.Provider))
            throw new Exception("DynamicLookupPageTransformFactory::AddDynamicOptions, No Provider name given in LookupSources");

        var lookupProvider = lookupProviders.Get(submitDetails.Provider);
        OptionsResponse lookupOptionsResult = await lookupProvider.GetAsync(request.Url, submitDetails.AuthToken);

        if (!lookupOptionsResult.Options.Any())
            throw new Exception("DynamicLookupPageTransformFactory::AddDynamicOptions, Provider returned no options");

        if (lookupOptionsResult.SelectExactly > lookupOptionsResult.Options.Count)
            throw new Exception("DynamicLookupPageTransformFactory::AddDynamicOptions, There cannot be less options available than the SelectExactly value");

        element.Properties.Options.AddRange(lookupOptionsResult.Options);

        if (lookupOptionsResult.SelectExactly > 0)
            element.Properties.SelectExactly = lookupOptionsResult.SelectExactly;

        if (lookupOptionsResult.SelectExactly > 1)
        {
            element.Properties.Hint = $"Select <b>{lookupOptionsResult.SelectExactly}</b> options.";
            element.Properties.CustomValidationMessage = $"Select {lookupOptionsResult.SelectExactly} options";
        }
        else if (lookupOptionsResult.SelectExactly is 1)
        {
            element.Properties.Hint = $"Select <b>{lookupOptionsResult.SelectExactly}</b> option.";
            element.Properties.CustomValidationMessage = $"Select {lookupOptionsResult.SelectExactly} option";
        }
    }
}