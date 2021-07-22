using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models;
using form_builder.Models.Elements;
using form_builder.Providers.Booking;
using form_builder.Services.MappingService.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.SubmissionActions
{
    public class PostSubmissionAction : IPostSubmissionAction
    {
        private readonly IEnumerable<IBookingProvider> _bookingProviders;

        public PostSubmissionAction(IEnumerable<IBookingProvider> bookingProviders)
        {
            _bookingProviders = bookingProviders;
        }

        public async Task ConfirmBooking(MappingEntity mappingEntity, FormSchema baseForm, string environmentName)
        {
            var booking = (Booking)baseForm.Pages
                .Where(page => page.Elements is not null)
                .SelectMany(page => page.Elements)
                .First(element => element.Type.Equals(EElementType.Booking));

            var bookingId = Convert.ToString(mappingEntity.FormAnswers.AllAnswers
                .ToDictionary(x => x.QuestionId, x => x.Response)
                .Where(_ => _.Key.Equals(booking.ReservedIdQuestionId))
                .FirstOrDefault().Value);

            var appointmentType = booking.Properties.AppointmentTypes
                .GetAppointmentTypeForEnvironment(environmentName);

            await _bookingProviders
                 .Get(booking.Properties.BookingProvider)
                 .Confirm(new()
                 {
                     BookingId = new Guid(bookingId),
                     AdditionalInformation = string.Empty,
                     OptionalResources = appointmentType.OptionalResources
                 });
        }
    }
}
