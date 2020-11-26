using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using form_builder.Constants;
using form_builder.Enum;
using form_builder.Helpers;
using form_builder.Helpers.ElementHelpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
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
        public string FormattedTimeForCheckYourBooking => SelectedBooking.IsFullDayAppointment ? $"between {DateTime.Today.Add(SelectedBooking.AppointmentTimes.First().StartTime).ToString("h:mm tt").Replace(" ", "").Replace(":00", "").ToLower()} and {DateTime.Today.Add(SelectedBooking.AppointmentTimes.First().EndTime).ToString("h:mm tt").Replace(" ", "").Replace(":00", "")}" : "NotFullDay";
        public AvailabilityDayResponse SelectedBooking;
        public bool IsSelectedAppointmentFullDay => SelectedBooking.IsFullDayAppointment;
        public bool IsAppointmentTypeFullDay => Appointments.Any(_ => _.IsFullDayAppointment);
        public string AppointmentTypeFullDayIAG => $"You can select a date for {FormName} but you can not select a time. We’ll be with you between {DateTime.Today.Add(Appointments.FirstOrDefault().AppointmentTimes.First().StartTime).ToString("h:mm tt").Replace(" ", "").Replace(":00", "").ToLower()} and {DateTime.Today.Add(Appointments.FirstOrDefault().AppointmentTimes.First().EndTime).ToString("h:mm tt").Replace(" ", "").Replace(":00", "").ToLower()}.";
        public string BookingDateQuestionId => $"{Properties.QuestionId}{BookingConstants.APPOINTMENT_DATE}";
        public DateTime CurrentSelectedMonth { get; set; }
        public string CurrentSelectedMonthText => $"{CurrentSelectedMonth:MMMMM yyyy}";
        public DateTime NextSelectableMonth => new DateTime(CurrentSelectedMonth.Year, CurrentSelectedMonth.Month, 1).AddMonths(1);
        public DateTime PreviousSelectableMonth => new DateTime(CurrentSelectedMonth.Year, CurrentSelectedMonth.Month, 1).AddMonths(-1);
        public bool DisplayNextMonthArrow => NextSelectableMonth <= new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(Properties.SearchPeriod);
        public bool DisplayPreviousMonthArrow => CurrentSelectedMonth.Year > DateTime.Now.Year 
            ? true 
            : CurrentSelectedMonth.Month > DateTime.Now.Month;

        public string FormName { get; set; }

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
            FormName = formSchema.FormName;
            var bookingInformation = (BookingInformation)results.First();

            switch (subPath as string)
            {
                case BookingConstants.CHECK_YOUR_BOOKING:
                    SelectedBooking = GetSelectedAppointment(bookingInformation);
                    return viewRender.RenderAsync("CheckYourBooking", this);
                default:
                    ConfigureDropDown(bookingInformation);
                    return viewRender.RenderAsync(Type.ToString(), this);
            }
        }
        private void ConfigureDropDown(BookingInformation results)
        {
            Appointments = results.Appointents;
            CurrentSelectedMonth = results.CurrentSearchedMonth;

            SelectList.Add(new SelectListItem("selet date", string.Empty));

            Appointments.ForEach(_ =>
            {
                SelectList.Add(new SelectListItem(_.Date.ToString(), _.Date.ToString()));
            });
        }

        private AvailabilityDayResponse GetSelectedAppointment(BookingInformation results)
        {
            Appointments = results.Appointents;
            var selectedAppointment = Appointments.FirstOrDefault(_ => _.Date.ToString().Equals(Properties.Value));

            if (selectedAppointment == null)
                throw new ApplicationException("Booking::RenderAsync, Unable to find selected appointment while attempting to generate check your booking view");

            return selectedAppointment;
        }
    }
}