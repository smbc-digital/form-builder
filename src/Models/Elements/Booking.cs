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

namespace form_builder.Models.Elements
{
    public class Booking : Element
    {
        public Booking()
        {
            Type = EElementType.Booking;
        }
        public List<SelectListItem> Appointments { get; set; } = new List<SelectListItem>();

        public string FormattedDateForCheckYourBooking => DateTime.Parse(Properties.Value).ToString("dddd dd MMMM yyyy");
        public string FormattedTimeForCheckYourBooking => SelectedBooking.IsFullDayAppointment ? $"between {DateTime.Today.Add(SelectedBooking.AppointmentTimes.First().StartTime).ToString("h:mm tt").Replace(" ", "").Replace(":00", "").ToLower()} and {DateTime.Today.Add(SelectedBooking.AppointmentTimes.First().EndTime).ToString("h:mm tt").Replace(" ", "").Replace(":00", "")}" : "NotFullDay";
        public AvailabilityDayResponse SelectedBooking;
        public bool IsAppointmentFullDay => SelectedBooking.IsFullDayAppointment;
        public string BookingDateQuestionId => $"{Properties.QuestionId}{BookingConstants.APPOINTMENT_DATE}";

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

            switch (subPath as string)
            {
                case BookingConstants.CHECK_YOUR_BOOKING:
                    SelectedBooking = GetSelectedAppointment(results);
                    return viewRender.RenderAsync("CheckYourBooking", this);
                default:
                    ConfigureDropDown(results);
                    return viewRender.RenderAsync(Type.ToString(), this);
            }
        }
        private void ConfigureDropDown(List<object> results)
        {
            var result = ConvertedAvailabilityDayResponse(results);
            Appointments.Add(new SelectListItem("selet date", string.Empty));

            result.ForEach(_ =>
            {
                Appointments.Add(new SelectListItem(_.Date.ToString(), _.Date.ToString()));
            });
        }


        private AvailabilityDayResponse GetSelectedAppointment(List<object> results)
        {
            var result = ConvertedAvailabilityDayResponse(results);

            var selectedAppointment = result.FirstOrDefault(_ => _.Date.ToString().Equals(Properties.Value));

            if (selectedAppointment == null)
                throw new ApplicationException("Booking::RenderAsync, Unable to find selected appointment while attempting to generate check your booking view");

            return selectedAppointment;
        }

        private List<AvailabilityDayResponse> ConvertedAvailabilityDayResponse(List<object> results) => results.Select(_ =>
        {
            AvailabilityDayResponse appointment;

            if ((_ as JObject) != null)
                appointment = (_ as JObject).ToObject<AvailabilityDayResponse>();
            else
                appointment = _ as AvailabilityDayResponse;

            return appointment;
        }).ToList();

    }
}