using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using form_builder.Models;
using form_builder.Services.BookingService.Entities;
using form_builder.Services.PageService.Entities;

namespace form_builder.Services.BookingService
{
    public interface IBookingService
    {
        Task<BookingProcessEntity> Get(
            string formName,
            Page currentPage,
            string guid);

        Task<ProcessRequestEntity> ProcessBooking(
            Dictionary<string, dynamic> viewModel,
            Page currentPage,
            FormSchema baseForm,
            string guid,
            string path);

        Task ProcessMonthRequest(
            Dictionary<string, object> viewModel,
            string form,
            string path);

        Task<CancelledAppointmentInformation> ValidateCancellationRequest(
            string formName,
            Guid bookingGuid, 
            string hash);

        Task Cancel(
            string formName,
            Guid bookingGuid,
            string hash);
    }
}
