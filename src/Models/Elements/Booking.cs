using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using StockportGovUK.NetStandard.Models.Booking.Response;
using static form_builder.Services.BookingService.BookingService;

namespace form_builder.Models.Elements
{
    public class Booking : Element
    {
        public Booking()
        {
            Type = EElementType.Booking;
        }
        public List<SelectListItem> SelectList { get; set; } = new List<SelectListItem>();
        public List<AvailabilityDayResponse> Appointments { get; set; } = new List<AvailabilityDayResponse>();
        public string FormattedDateForCheckYourBooking => DateTime.Parse(Properties.Value).ToString("dddd dd MMMM yyyy");
        public string FormattedTimeForCheckYourBooking => SelectedBooking.IsFullDayAppointment ? $"between {AppointmentStartTime} and {AppointmentEndTime}" : "NotFullDay";
        public AvailabilityDayResponse SelectedBooking;
        public bool IsSelectedAppointmentFullDay => SelectedBooking.IsFullDayAppointment;
        public bool IsAppointmentTypeFullDay { get; set; }
        public string AppointmentStartTime { get; set; }
        public string AppointmentEndTime { get; set; }
        public string AppointmentTypeFullDayIAG => $"You can select a date for {FormName} but you can not select a time. We’ll be with you between {AppointmentStartTime} and {AppointmentEndTime}.";
        public string BookingDateQuestionId => $"{Properties.QuestionId}{BookingConstants.APPOINTMENT_DATE}";
        public DateTime CurrentSelectedMonth { get; set; }
        public DateTime FirstAvailableMonth { get; set; }
        public bool DisplayNextAvailableAppointmentIAG => FirstAvailableMonth.Date > DateTime.Now.Date && CurrentSelectedMonth.Month == FirstAvailableMonth.Month && CurrentSelectedMonth.Year == FirstAvailableMonth.Year;
        public string CurrentSelectedMonthText => $"{CurrentSelectedMonth:MMMMM yyyy}";
        public DateTime NextSelectableMonth => new DateTime(CurrentSelectedMonth.Year, CurrentSelectedMonth.Month, 1).AddMonths(1);
        public DateTime PreviousSelectableMonth => new DateTime(CurrentSelectedMonth.Year, CurrentSelectedMonth.Month, 1).AddMonths(-1);
        public bool DisplayNextMonthArrow => NextSelectableMonth <= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(Properties.SearchPeriod);
        public bool DisplayPreviousMonthArrow => CurrentSelectedMonth > FirstAvailableMonth;
        public override string GetLabelText() => $"Booking{(Properties.Optional ? " (optional)" : string.Empty)}";
        public string FormName { get; set; }
        public string ReservedBookingId { get; set; }
        public string ReservedBookingDate { get; set; }
        public string MonthSelectionPostUrl { get; set; }

        public override Task<string> RenderAsync(IViewRender viewRender,
            IElementHelper elementHelper,
            string guid,
            Dictionary<string, dynamic> viewModel,
            Page page,
            FormSchema formSchema,
            IWebHostEnvironment environment,
            FormAnswers formAnswers,
            List<object> results = null)
        {
            viewModel.TryGetValue(LookUpConstants.SubPathViewModelKey, out var subPath);
            Properties.Value = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, BookingConstants.APPOINTMENT_DATE);
            ReservedBookingId = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, $"-{BookingConstants.RESERVED_BOOKING_ID}");
            ReservedBookingDate = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, $"-{BookingConstants.RESERVED_BOOKING_DATE}");
            FormName = formSchema.FormName;
            ConfigureBookingInformation(results);

            switch (subPath as string)
            {
                case BookingConstants.CHECK_YOUR_BOOKING:
                    SelectedBooking = GetSelectedAppointment();
                    return viewRender.RenderAsync("CheckYourBooking", this);
                default:
                    ConfigureDropDown();
                    MonthSelectionPostUrl = formSchema.BaseURL.ToBookingRequestedMonthUrl(page.PageSlug);
                    return viewRender.RenderAsync(Type.ToString(), this);
            }
        }

        private void ConfigureBookingInformation(List<object> results)
        {
            var bookingInformation = (BookingInformation)results.First();

            Appointments = bookingInformation.Appointments;
            CurrentSelectedMonth = bookingInformation.CurrentSearchedMonth;
            FirstAvailableMonth = bookingInformation.FirstAvailableMonth;
            IsAppointmentTypeFullDay = bookingInformation.IsFullDayAppointment;
            if (bookingInformation.IsFullDayAppointment)
            {
                AppointmentStartTime = bookingInformation.AppointmentStartTime;
                AppointmentEndTime = bookingInformation.AppointmentEndTime;
            }
        }

        private void ConfigureDropDown()
        {
            SelectList.Add(new SelectListItem("selet date", string.Empty));

            Appointments.ForEach(_ =>
            {
                SelectList.Add(new SelectListItem(_.Date.ToString(), _.Date.ToString()));
            });
        }

        private AvailabilityDayResponse GetSelectedAppointment()
        {
            var selectedAppointment = Appointments.FirstOrDefault(_ => _.Date.ToString().Equals(Properties.Value));

            if (selectedAppointment == null)
                throw new ApplicationException("Booking::RenderAsync, Unable to find selected appointment while attempting to generate check your booking view");

            return selectedAppointment;
        }
    }
}