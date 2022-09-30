using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers.ActionsHelpers;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Models.Properties.ElementProperties;
using form_builder.Providers.Lookup;
using form_builder.Services.RetrieveExternalDataService.Entities;
using StockportGovUK.NetStandard.Gateways.Models.FormBuilder;

namespace form_builder.Factories.Transform.UserSchema;

public class BookingLookupPageTransformFactory : IUserPageTransformFactory
{
    private readonly IActionHelper _actionHelper;
    private readonly IEnumerable<ILookupProvider> _lookupProviders;
    private readonly IWebHostEnvironment _environment;

    public BookingLookupPageTransformFactory(IActionHelper actionHelper,
        IEnumerable<ILookupProvider> lookupProviders,
        IWebHostEnvironment environment)
    {
        _actionHelper = actionHelper;
        _lookupProviders = lookupProviders;
        _environment = environment;
    }

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
                .Equals(_environment.EnvironmentName, StringComparison.OrdinalIgnoreCase));

        if (submitDetails is null)
            throw new Exception(
                $"{nameof(BookingLookupPageTransformFactory)}::{nameof(AddDynamicAppointmentTypes)}: No LookUpSource found for environment");

        RequestEntity request = _actionHelper.GenerateUrl(submitDetails.URL, convertedAnswers);

        if (string.IsNullOrEmpty(submitDetails.Provider))
            throw new Exception(
                $"{nameof(BookingLookupPageTransformFactory)}::{nameof(AddDynamicAppointmentTypes)}: No Provider name given in LookupSources");

        var lookupProvider = _lookupProviders.Get(submitDetails.Provider);
        List<AppointmentType> lookupAppointmentResult = await lookupProvider.GetAppointmentTypesAsync(request.Url, submitDetails.AuthToken);

        if (!lookupAppointmentResult.Any())
            throw new Exception(
                $"{nameof(BookingLookupPageTransformFactory)}::{nameof(AddDynamicAppointmentTypes)}: Provider returned no AppointmentTypes");

        element.Properties.AppointmentTypes = lookupAppointmentResult;
    }
}