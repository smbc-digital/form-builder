using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Helpers.Session;
using form_builder.Services.MappingService;
using form_builder.Services.PayService;
using form_builder.ViewModels;
using form_builder.Workflows.SuccessWorkflow;
using Microsoft.AspNetCore.Mvc;

namespace form_builder.Controllers.Payment
{
    public class PaymentController : Controller
    {
        private readonly IPayService _payService;
        private readonly ISessionHelper _sessionHelper;
        private readonly IMappingService _mappingService;
        private readonly ISuccessWorkflow _successWorkflow;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IPayService payService, ISessionHelper sessionHelper, IMappingService mappingService, ISuccessWorkflow successWorkflow, ILogger<PaymentController> logger)
        {
            _payService = payService;
            _sessionHelper = sessionHelper;
            _mappingService = mappingService;
            _successWorkflow = successWorkflow;
            _logger = logger;
        }

        [HttpGet]
        [Route("{form}/{path}/payment-response")]
        public async Task<IActionResult> HandlePaymentResponse(string form, string path, [FromQuery] string responseCode, [FromQuery] string callingAppTxnRef)
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
            catch (PaymentCallbackException)
            {
                return RedirectToAction("CallbackFailure", new
                {
                    form,
                    reference = callingAppTxnRef
                });
            }
        }

        [HttpGet]
        [Route("{form}/payment-success")]
        public async Task<IActionResult> PaymentSuccess(string form, [FromQuery] string reference)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();
            _logger.LogInformation($"PaymentController:PaymentSuccess:{sessionGuid} {form} Attempting payment success {reference}");

            var result = await _successWorkflow.Process(EBehaviourType.SubmitAndPay, form);

            var success = new SuccessViewModel
            {
                Reference = reference,
                PageContent = result.HtmlContent,
                FormName = result.FormName,
                StartPageUrl = result.StartPageUrl,
                Embeddable = result.Embeddable,
                PageTitle = result.PageTitle,
                BannerTitle = result.BannerTitle,
                LeadingParagraph = result.LeadingParagraph
            };

            return View("../Home/Success", success);
        }

        [HttpGet]
        [Route("{form}/payment-failure")]
        public async Task<IActionResult> PaymentFailure(string form, [FromQuery] string reference)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();
            var data = await _mappingService.Map(sessionGuid, form);
            var url = await _payService.ProcessPayment(data, form, "payment", reference, sessionGuid);

            var paymentFailureViewModel = new PaymentFailureViewModel
            {
                FormName = data.BaseForm.FormName,
                PageTitle = "Failure",
                Reference = reference,
                PaymentUrl = url,
                StartPageUrl = data.BaseForm.StartPageUrl,
                Embeddable = data.BaseForm.Embeddable
            };

            return View("./Failure", paymentFailureViewModel);
        }

        [HttpGet]
        [Route("{form}/payment-declined")]
        public async Task<IActionResult> PaymentDeclined(string form, [FromQuery] string reference)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();
            _logger.LogWarning($"PaymentController:PaymentDeclined:{sessionGuid} {form} Payment declined {reference}");

            var data = await _mappingService.Map(sessionGuid, form);
            var url = await _payService.ProcessPayment(data, form, "payment", reference, sessionGuid);
            var paymentDeclinedViewModel = new PaymentFailureViewModel
            {
                FormName = data.BaseForm.FormName,
                PageTitle = "Declined",
                Reference = reference,
                PaymentUrl = url,
                StartPageUrl = data.BaseForm.StartPageUrl,
                Embeddable = data.BaseForm.Embeddable
            };

            return View("./Declined", paymentDeclinedViewModel);
        }

        [HttpGet]
        [Route("{form}/callback-failure")]
        public async Task<IActionResult> CallbackFailure(string form, [FromQuery] string reference)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();
            _logger.LogWarning($"PaymentController:PaymentFailure:{sessionGuid} {form} Payment failure {reference}");

            var data = await _mappingService.Map(sessionGuid, form);

            var callbackFailureViewModel = new CallbackFailureViewModel
            {
                FormName = data.BaseForm.FormName,
                PageTitle = "Error",
                Reference = reference,
                StartPageUrl = data.BaseForm.StartPageUrl,
                Embeddable = data.BaseForm.Embeddable,
                CallbackFailureContactNumber = data.BaseForm.CallbackFailureContactNumber
            };

            return View("./CallbackFailure", callbackFailureViewModel);
        }
    }
}