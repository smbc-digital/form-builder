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
        public string BookingDateQuestionId => $"{Properties.QuestionId}{BookingConstants.APPOINTMENT_DATE}";
        public string BookingTimeQuestionId => $"{Properties.QuestionId}{BookingConstants.APPOINTMENT_DATE}{BookingConstants.APPOINTMENT_TIME}";
        public string BookingAppointmentId => $"{Properties.QuestionId}{BookingConstants.APPOINTMENT_DATE}{BookingConstants.APPOINTMENT_ID}";

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
            var fullDayStartTime = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, $"{BookingConstants.APPOINTMENT_DATE}{BookingConstants.APPOINTMENT_FULL_DAY_START_TIME}");
            var fullDayEndTime = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers, $"{BookingConstants.APPOINTMENT_DATE}{BookingConstants.APPOINTMENT_FULL_DAY_END_TIME}");
            switch (subPath as string)
            {
                case BookingConstants.CHECK_YOUR_BOOKING:
                    return viewRender.RenderAsync("CheckYourBooking", this);
                default:
                    Appointments.Add(new SelectListItem("selet date", string.Empty));
                    results.ForEach((objectResult) =>
                    {
                        AvailabilityDayResponse appointment;

                        if ((objectResult as JObject) != null)
                            appointment = (objectResult as JObject).ToObject<AvailabilityDayResponse>();
                        else
                            appointment = objectResult as AvailabilityDayResponse;

                        var value = appointment.IsFullDayAppointment 
                            ? $"{appointment.Date.ToString()}|{appointment.AppointmentTimes.First().StartTime}|{appointment.AppointmentTimes.First().EndTime}" 
                            : appointment.Date.ToString();

                        Appointments.Add(new SelectListItem(appointment.Date.ToString(), value));
                    });

                    return viewRender.RenderAsync(Type.ToString(), this);
            }
        }
    }
}