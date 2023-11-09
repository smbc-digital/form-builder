using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.Booking;
using form_builder.Services.MappingService.Entities;
using StockportGovUK.NetStandard.Gateways.Enums;
using StockportGovUK.NetStandard.Gateways.Models.Booking.Request;

namespace form_builder.Helpers.Submit;

public class SubmitHelper : ISubmitHelper
{
    private readonly IEnumerable<IBookingProvider> _bookingProviders;

    public SubmitHelper(IEnumerable<IBookingProvider> bookingProviders) =>
        _bookingProviders = bookingProviders;

    public async Task ConfirmBookings(MappingEntity mappingEntity, string environmentName, string caseReference)
    {
        List<Page> journeyPages = mappingEntity.BaseForm.GetReducedPages(mappingEntity.FormAnswers);

        List<Page> autoConfirmBookingPages = journeyPages.FindAll(page => page.Elements
            .Any(element => element.Type.Equals(EElementType.Booking) && element.Properties.AutoConfirm));

        foreach (var page in autoConfirmBookingPages)
        {
            var booking = (Booking)page.Elements.FirstOrDefault(element => element.Type.Equals(EElementType.Booking));
            var bookingId = mappingEntity.FormAnswers.AllAnswers
                .FirstOrDefault(_ => _.QuestionId.Equals(booking.ReservedIdQuestionId)).Response;

            var confirmationRequest = new ConfirmationRequest
            {
                BookingId = new Guid(bookingId)
            };

            if (!string.IsNullOrEmpty(caseReference))
                confirmationRequest.ForeignReferences = new List<AddReferenceRequest>
                {
                    new ()
                    {
                        BookingId = new Guid(bookingId),
                        Description = EBookingForeignReference.VerintRef.ToString(),
                        Reference = caseReference
                    }
                };

            await _bookingProviders
                .Get(booking.Properties.BookingProvider)
                .Confirm(confirmationRequest);
        }
    }
}
