namespace form_builder.Controllers.Payment;

public class PaymentController(IPayService payService,
    ISessionHelper sessionHelper,
    IMappingService mappingService,
    ISuccessWorkflow successWorkflow,
    ILogger<PaymentController> logger)
    : Controller
{
    [HttpGet]
    [Route("{form}/{path}/payment-response")]
    public async Task<IActionResult> HandlePaymentResponse(string form, string path, [FromQuery] string responseCode, [FromQuery] string callingAppTxnRef)
    {
        try
        {
            var reference = await payService.ProcessPaymentResponse(form, responseCode, callingAppTxnRef);

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
        var result = await successWorkflow.Process(EBehaviourType.SubmitAndPay, form);

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
        string browserSessionId = sessionHelper.GetBrowserSessionId();
        string cacheKey = $"{form}::{browserSessionId}";
        MappingEntity data = await mappingService.Map(cacheKey, form, null, null);
        string url = await payService.ProcessPayment(data, form, "payment", reference, cacheKey);

        PaymentFailureViewModel paymentFailureViewModel = new()
        {
            FormName = data.BaseForm.FormName,
            PageTitle = "Failure",
            Reference = reference,
            PaymentUrl = url,
            StartPageUrl = data.BaseForm.StartPageUrl,
            Embeddable = data.BaseForm.Embeddable,
            PaymentIssueButtonUrl = data.BaseForm.PaymentIssueButtonUrl,
            PaymentIssueButtonLabel = data.BaseForm.PaymentIssueButtonLabel,
            HideBackButton = true
        };

        return View("./Failure", paymentFailureViewModel);
    }

    [HttpGet]
    [Route("{form}/payment-declined")]
    public async Task<IActionResult> PaymentDeclined(string form, [FromQuery] string reference)
    {
        string browserSessionId = sessionHelper.GetBrowserSessionId();
        string cacheKey = $"{form}::{browserSessionId}";
        MappingEntity data = await mappingService.Map(cacheKey, form, null, null);
        string url = await payService.ProcessPayment(data, form, "payment", reference, cacheKey);
        PaymentFailureViewModel paymentDeclinedViewModel = new()
        {
            FormName = data.BaseForm.FormName,
            PageTitle = "Declined",
            Reference = reference,
            PaymentUrl = url,
            StartPageUrl = data.BaseForm.StartPageUrl,
            Embeddable = data.BaseForm.Embeddable,
            PaymentIssueButtonUrl = data.BaseForm.PaymentIssueButtonUrl,
            PaymentIssueButtonLabel = data.BaseForm.PaymentIssueButtonLabel,
            HideBackButton = true
        };

        return View("./Declined", paymentDeclinedViewModel);
    }

    [HttpGet]
    [Route("{form}/callback-failure")]
    public async Task<IActionResult> CallbackFailure(string form, [FromQuery] string reference)
    {
        string browserSessionId = sessionHelper.GetBrowserSessionId();
        string cacheKey = $"{form}::{browserSessionId}";
        logger.LogWarning($"PaymentController:PaymentFailure: {cacheKey} Payment failure {reference}");

        var data = await mappingService.Map(cacheKey, form, null, null);

        var callbackFailureViewModel = new CallbackFailureViewModel
        {
            FormName = data.BaseForm.FormName,
            PageTitle = "Error",
            Reference = reference,
            StartPageUrl = data.BaseForm.StartPageUrl,
            Embeddable = data.BaseForm.Embeddable,
            CallbackFailureContactNumber = data.BaseForm.CallbackFailureContactNumber,
            PaymentIssueButtonUrl = data.BaseForm.PaymentIssueButtonUrl,
            PaymentIssueButtonLabel = data.BaseForm.PaymentIssueButtonLabel,
            HideBackButton = true
        };

        return View("./CallbackFailure", callbackFailureViewModel);
    }
}