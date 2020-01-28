using form_builder.Services.PayService;
using Microsoft.AspNetCore.Mvc;

namespace form_builder.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPayService _payService;

        public PaymentController(IPayService payService)
        {
            _payService = payService;
        }

        [HttpGet]
        [Route("{form}/{path}/payment-response")]
        public IActionResult HandlePaymentResponse(string form, string path, [FromQuery]string responseCode, [FromQuery]string callingAppTxnRef)
        {
            var reference = _payService.ProcessPaymentResponse(form, responseCode);

            return RedirectToAction("Success", "Home", new
            {
                path = "success",
                form,
                reference
            });
        }
    }
}