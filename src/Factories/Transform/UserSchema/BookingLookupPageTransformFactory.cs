namespace form_builder.Factories.Transform.UserSchema;

public class BookingLookupPageTransformFactory(IActionHelper actionHelper,
    IEnumerable<ILookupProvider> lookupProviders,
    IWebHostEnvironment environment)
    : IUserPageTransformFactory
{
    public async Task<Page> Transform(Page page, FormAnswers convertedAnswers)
    {
        if (page.HasDynamicLookupElements)
        {
            foreach (var element in page.Elements.Where(_ => _.Type.Equals(EElementType.Booking) &&
                         !string.IsNullOrEmpty(_.Lookup) && _.Lookup.Equals(LookUpConstants.Dynamic)))
            {
                await AddDynamicAppointmentTypes(element, convertedAnswers);
            }
        }

        return page;
    }

    private async Task AddDynamicAppointmentTypes(IElement element, FormAnswers convertedAnswers)
    {
        LookupSource submitDetails = element.Properties.LookupSources
            .SingleOrDefault(x => x.EnvironmentName
                .Equals(environment.EnvironmentName, StringComparison.OrdinalIgnoreCase));

        if (submitDetails is null)
            throw new Exception(
                $"{nameof(BookingLookupPageTransformFactory)}::{nameof(AddDynamicAppointmentTypes)}: No LookUpSource found for environment");

        RequestEntity request = actionHelper.GenerateUrl(submitDetails.URL, convertedAnswers);

        if (string.IsNullOrEmpty(submitDetails.Provider))
            throw new Exception(
                $"{nameof(BookingLookupPageTransformFactory)}::{nameof(AddDynamicAppointmentTypes)}: No Provider name given in LookupSources");

        var lookupProvider = lookupProviders.Get(submitDetails.Provider);
        List<AppointmentType> lookupAppointmentResult = await lookupProvider.GetAppointmentTypesAsync(request.Url, submitDetails.AuthToken);

        if (!lookupAppointmentResult.Any())
            throw new Exception(
                $"{nameof(BookingLookupPageTransformFactory)}::{nameof(AddDynamicAppointmentTypes)}: Provider returned no AppointmentTypes");

        element.Properties.AppointmentTypes = lookupAppointmentResult;
    }
}