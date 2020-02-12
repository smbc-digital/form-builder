using System.Threading.Tasks;
using form_builder.Exceptions;
using form_builder.Helpers.Session;
using form_builder.Services.PayService;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace form_builder.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IPayService _payService;
        private readonly ISessionHelper _sessionHelper;

        public PaymentController(IPayService payService, ISessionHelper sessionHelper)
        {
            _payService = payService;
            _sessionHelper = sessionHelper;
        }

        [HttpGet]
        [Route("{form}/{path}/payment-response")]
        public async Task<IActionResult> HandlePaymentResponse(string form, string path, [FromQuery]string responseCode, [FromQuery]string callingAppTxnRef)
        {
            try
            {
                var reference = await _payService.ProcessPaymentResponse(form, responseCode, callingAppTxnRef);

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
                    form,
                    reference = callingAppTxnRef
                });
            }
            catch (PaymentDeclinedException)
            {
                return RedirectToAction("PaymentDeclined", new
                {
                    form,
                    reference = callingAppTxnRef
                });
            }
        }

        [HttpGet]
        [Route("{form}/payment-success")]
        public IActionResult PaymentSuccess(string form, [FromQuery] string reference)
        {
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
        public async Task<IActionResult> PaymentFailure(string form, [FromQuery] string reference)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();
            var path = "payment";
            var url = await _payService.ProcessPayment(form, path, reference, sessionGuid);
            var paymentFailureViewModel = new PaymentFailureViewModel
            {
                FormName = form,
                PageTitle = "Failure",
                Reference = reference,
                PaymentUrl = url
            };

            return View("./Failure", paymentFailureViewModel);
        }

        [HttpGet]
        [Route("{form}/payment-declined")]
        public async Task<IActionResult> PaymentDeclined(string form, [FromQuery] string reference)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();
            var path = "payment";
            var url = await _payService.ProcessPayment(form, path, reference, sessionGuid);
            var paymentDeclinedViewModel = new PaymentDeclinedViewModel
            {
                FormName = form,
                PageTitle = "Declined",
                Reference = reference,
                PaymentUrl = url.ToString()
            };

            return View("./Declined", paymentDeclinedViewModel);
        }

        [HttpGet]
        [Route("{form}/payment-summary")]
        public IActionResult PaymentSummary(string form, [FromQuery] string reference)
        {
            var paymentSummaryViewModel = new PaymentSummaryViewModel
            {
                FormName = form,
                PageTitle = "Summary",
                Amount = "12", // need to get the amount from congif
                Description = "bin" // need to get the description from config
            };

            return View("./Summary", paymentSummaryViewModel);
        }
    }
}