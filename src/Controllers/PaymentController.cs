using form_builder.Exceptions;
using form_builder.Services.PayService;
using form_builder.ViewModels;
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
            try
            {
                var reference = _payService.ProcessPaymentResponse(form, responseCode);

                return RedirectToAction("PaymentSuccess", new
                {
                    form,
                    reference
                });
            }
            catch (PaymentFailureException)
            {
                return RedirectToAction("PaymentFailure", new
                {
                    form
                });
            }
            catch (PaymentDeclinedException)
            {
                return RedirectToAction("PaymentFailure", new
                {
                    form
                });
            }
        }

        [HttpGet]
        [Route("{form}/payment-success")]
        public IActionResult PaymentSuccess(string form, [FromQuery] string reference)
        {
            //var result = await _submitWorkflow.Submit(form);
            var paymentSuccessViewModel = new PaymentSuccessViewModel
            {
                Reference = reference,
                FormName = form,
                PageTitle = "Success"
            };

            return View("./Index", paymentSuccessViewModel);
        }

        [HttpGet]
        [Route("{form}/payment-failure")]
        public IActionResult PaymentFailure(string form)
        {
            var paymentFailureViewModel = new PaymentFailureViewModel 
            {
                FormName = form,
                PageTitle = "Failure",
            };

            return View("./Failure", paymentFailureViewModel);
        }
    }
}