using System.Collections.Generic;
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

        public string BookingDateQuestionId => $"{Properties.QuestionId}{BookingConstants.APPOINTMENT_DATE}";
        public string BookingTimeQuestionId => $"{Properties.QuestionId}{BookingConstants.APPOINTMENT_TIME}";
        public string BookingAppointmentId => $"{Properties.QuestionId}{BookingConstants.APPOINTMENT_ID}";

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
            switch (subPath as string)
            {
                case BookingConstants.CHECK_YOUR_BOOKING:
                    return viewRender.RenderAsync("CheckYourBooking", this);
                default:
                    Properties.Value = elementHelper.CurrentValue(Properties.QuestionId, viewModel, formAnswers);

                    Appointments.Add(new SelectListItem("selet date", string.Empty));
                    results.ForEach((objectResult) =>
                    {
                        AvailabilityDayResponse appointment;

                        if ((objectResult as JObject) != null)
                            appointment = (objectResult as JObject).ToObject<AvailabilityDayResponse>();
                        else
                            appointment = objectResult as AvailabilityDayResponse;

                        Appointments.Add(new SelectListItem(appointment.Date.ToString(), appointment.Date.ToString()));
                    });

                    return viewRender.RenderAsync(Type.ToString(), this);
            }
        }
    }
}