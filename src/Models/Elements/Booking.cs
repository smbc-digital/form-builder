using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Extensions;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using form_builder.Models.Booking;
using form_builder.Utils.Extesions;
using Microsoft.AspNetCore.Hosting;
using StockportGovUK.NetStandard.Models.Booking.Response;

namespace form_builder.Models.Elements
{
    public class Booking : Element
    {
        public Booking() => Type = EElementType.Booking;
        public List<CalendarDay> Calendar { get; set; } = new List<CalendarDay>();
        public List<AvailabilityDayResponse> Appointments { get; set; } = new List<AvailabilityDayResponse>();
        public string FormattedDateForCheckYourBooking => DateTime.Parse(Properties.Value).ToFullDateFormat();
        public string FormattedTimeForCheckYourBooking => SelectedBooking.IsFullDayAppointment ? $"between {AppointmentStartTime.ToTimeFormat()} and {AppointmentEndTime.ToTimeFormat()}" : "NotFullDay";
        public AvailabilityDayResponse SelectedBooking;
        public bool IsAppointmentTypeFullDay { get; set; }
        public DateTime AppointmentStartTime { get; set; }
        public DateTime AppointmentEndTime { get; set; }
        public DateTime CurrentSelectedMonth { get; set; }
        public DateTime FirstAvailableMonth { get; set; }
        public string AppointmentTypeFullDayIAG => $"You can select a date for {FormName} but you can not select a time. We’ll be with you between {AppointmentStartTime.ToTimeFormat()} and {AppointmentEndTime.ToTimeFormat()}.";
        public bool DisplayNextAvailableAppointmentIAG => FirstAvailableMonth.Date > DateTime.Now.Date && CurrentSelectedMonth.Month == FirstAvailableMonth.Month && CurrentSelectedMonth.Year == FirstAvailableMonth.Year;
        public string CurrentSelectedMonthText => $"{CurrentSelectedMonth:MMMMM yyyy}";
        public DateTime NextSelectableMonth => new DateTime(CurrentSelectedMonth.Year, CurrentSelectedMonth.Month, 1).AddMonths(1);
        public DateTime PreviousSelectableMonth => new DateTime(CurrentSelectedMonth.Year, CurrentSelectedMonth.Month, 1).AddMonths(-1);
        public bool DisplayNextMonthArrow => NextSelectableMonth <= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(Properties.SearchPeriod);
        public bool DisplayPreviousMonthArrow => new DateTime(CurrentSelectedMonth.Date.Year, CurrentSelectedMonth.Date.Month, 1) > FirstAvailableMonth;
        public override string GetLabelText() => $"Booking{(Properties.Optional ? " (optional)" : string.Empty)}";
        public string FormName { get; set; }
        public string ReservedBookingId { get; set; }
        public string ReservedBookingDate { get; set; }
        public string ReservedBookingStartTime { get; set; }
        public string ReservedBookingEndTime { get; set; }
        public string StartAppointmentTime { get; set; }
        public string EndAppointmentTime { get; set; }
        public string MonthSelectionPostUrl { get; set; }
        public string DateQuestionId => $"{Properties.QuestionId}-{BookingConstants.APPOINTMENT_DATE}";
        public string StartTimeQuestionId => $"{Properties.QuestionId}-{BookingConstants.APPOINTMENT_START_TIME}";
        public string EndTimeQuestionId => $"{Properties.QuestionId}-{BookingConstants.APPOINTMENT_END_TIME}";
        public string ReservedDateQuestionId => $"{Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_DATE}";
        public string ReservedStartTimeQuestionId => $"{Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_START_TIME}";
        public string ReservedEndTimeQuestionId => $"{Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_END_TIME}";
        public string ReservedIdQuestionId => $"{Properties.QuestionId}-{BookingConstants.RESERVED_BOOKING_ID}";

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

            ConfigureBookingInformation(results);
            FormName = formSchema.FormName;
            
            Properties.Value = elementHelper.CurrentValue(DateQuestionId, viewModel, formAnswers);

            if(string.IsNullOrEmpty(Properties.Value) && !DisplayPreviousMonthArrow)
                Properties.Value = Appointments.OrderBy(_ => _.Date).FirstOrDefault()?.Date.ToString();

            ReservedBookingId = elementHelper.CurrentValue(ReservedIdQuestionId, viewModel, formAnswers);
            ReservedBookingDate = elementHelper.CurrentValue(ReservedDateQuestionId, viewModel, formAnswers);
            ReservedBookingStartTime = IsAppointmentTypeFullDay ? AppointmentStartTime.ToString() : elementHelper.CurrentValue(ReservedStartTimeQuestionId, viewModel, formAnswers);
            ReservedBookingEndTime = IsAppointmentTypeFullDay ? AppointmentEndTime.ToString() : elementHelper.CurrentValue(ReservedEndTimeQuestionId, viewModel, formAnswers);
            StartAppointmentTime = IsAppointmentTypeFullDay ? AppointmentStartTime.ToString() : elementHelper.CurrentValue(StartTimeQuestionId, viewModel, formAnswers);                      
            EndAppointmentTime = IsAppointmentTypeFullDay ? AppointmentEndTime.ToString() : elementHelper.CurrentValue(EndTimeQuestionId, viewModel, formAnswers);                      

            switch (subPath as string)
            {
                case BookingConstants.CHECK_YOUR_BOOKING:
                    SelectedBooking = GetSelectedAppointment();
                    return viewRender.RenderAsync("CheckYourBooking", this);
                default:
                    CreateCalendar();
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

        private void CreateCalendar()
        {
            var dates = new List<CalendarDay>();
            var daysInMonth = DateTime.DaysInMonth(CurrentSelectedMonth.Year, CurrentSelectedMonth.Month);
            var daysToAdd = new DateTime(CurrentSelectedMonth.Year, CurrentSelectedMonth.Month, 1).PreviousDaysInMonth();

            for (int i = 0; i < daysToAdd; i++)
            {
                dates.Add(new CalendarDay{ IsDisabled = true, IsNotWithinCurrentSelectedMonth = true });
            }

            for (int i = 0; i < daysInMonth; i++)
            {
                var day = new DateTime(CurrentSelectedMonth.Year, CurrentSelectedMonth.Month, i + 1);
                var containsDay = Appointments.FirstOrDefault(_ => _.Date.Equals(day));
                dates.Add(new CalendarDay{ Date = day, IsDisabled = containsDay == null, Checked = containsDay != null && Properties.Value.Equals(day.Date.ToString())});
            }

            for (int i = dates.Count + 1; i < 43; i++)
            {
                dates.Add(new CalendarDay{ IsDisabled = true, IsNotWithinCurrentSelectedMonth = true });
            }
            Calendar = dates;
        }

        private AvailabilityDayResponse GetSelectedAppointment()
        {
            var selectedAppointment = Appointments.FirstOrDefault(_ => _.Date.ToString().Equals(Properties.Value));

            if (selectedAppointment == null)
                throw new ApplicationException("Booking::GetSelectedAppointment, Unable to find selected appointment while attempting to generate check your booking view");

            return selectedAppointment;
        }

    }
}