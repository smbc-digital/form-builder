using Microsoft.AspNetCore.Mvc;
using System;

namespace form_builder.Controllers
{
    public class PaymentController : Controller
    {

        public PaymentController()
        {
        }

        [HttpGet]
        [Route("{form}/{path}/payment-response")]
        public IActionResult HandlePaymentResponse(string form, string path, [FromQuery]string responseCode, [FromQuery]string callingAppTxnRef)
        {
            //Not currently handled.
            if (responseCode != "00000")
            {
                throw new Exception("Payment failed");
            }

            return RedirectToAction("Success", new
            {
                path
            });
        }
    }
}