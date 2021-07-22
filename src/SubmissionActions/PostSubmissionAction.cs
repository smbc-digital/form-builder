using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Models.Elements;
using form_builder.Providers.Booking;
using form_builder.Services.MappingService.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace form_builder.SubmissionActions
{
    public class PostSubmissionAction : IPostSubmissionAction
    {
        private readonly IEnumerable<IBookingProvider> _bookingProviders;
        private readonly ILogger<BookingProvider> _logger;

        public PostSubmissionAction(IEnumerable<IBookingProvider> bookingProviders, ILogger<BookingProvider> logger)
        {
            _bookingProviders = bookingProviders;
            _logger = logger;
        }

        public async Task ConfirmResult(MappingEntity mappingEntity, string environmentName)
        {
            try
            {
                var journeyPages = mappingEntity.BaseForm.GetReducedPages(mappingEntity.FormAnswers);

                var isAutoConfirm = journeyPages.Any(_ => _.Elements.Any(_ => _.Type.Equals(EElementType.Booking) && _.Properties.AutoConfirm.Equals(true)));
                if (isAutoConfirm)
                {
                    var booking = (Booking)mappingEntity.BaseForm.Pages
                    .Where(page => page.Elements is not null)
                    .SelectMany(page => page.Elements)
                    .FirstOrDefault(element => element.Type.Equals(EElementType.Booking));

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
            catch (ApplicationException ex)
            {
                _logger.LogError($"PostSubmissionAction: Failed to confirm booking , Exception: {ex}");
            }
        }
    }
}
