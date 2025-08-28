using form_builder.Enum;
using form_builder.Exceptions;
using form_builder.Helpers.Session;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using form_builder.Services.PayService;
using form_builder.ViewModels;
using form_builder.Workflows.SuccessWorkflow;
using Microsoft.AspNetCore.Mvc;

namespace form_builder.Controllers.Payment;

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
        string browserSessionId = _sessionHelper.GetBrowserSessionId();
        string cacheKey = $"{form}::{browserSessionId}";
        MappingEntity data = await _mappingService.Map(cacheKey, form, null, null);
        string url = await _payService.ProcessPayment(data, form, "payment", reference, cacheKey);

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
        string browserSessionId = _sessionHelper.GetBrowserSessionId();
        string cacheKey = $"{form}::{browserSessionId}";
        MappingEntity data = await _mappingService.Map(cacheKey, form, null, null);
        string url = await _payService.ProcessPayment(data, form, "payment", reference, cacheKey);
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
        string browserSessionId = _sessionHelper.GetBrowserSessionId();
        string cacheKey = $"{form}::{browserSessionId}";
        _logger.LogWarning($"PaymentController:PaymentFailure: {cacheKey} Payment failure {reference}");

        var data = await _mappingService.Map(cacheKey, form, null, null);

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