using System.Collections.Generic;
using System.Threading.Tasks;
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