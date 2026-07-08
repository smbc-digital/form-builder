namespace form_builder.Controllers.Payment;

public class FakePaymentController(IHttpContextAccessor httpContextAccessor,
    IWebHostEnvironment environment,
    IOptions<PaymentConfiguration> paymentConfiguration)
    : Controller
{
    private readonly PaymentConfiguration _paymentConfiguration = paymentConfiguration.Value;

    [HttpGet]
    [Route("{form}/{path}/fake-payment")]
    public IActionResult Index([FromRoute] string form, [FromRoute] string path, [FromQuery] string reference, [FromQuery] string amount)
    {
        return _paymentConfiguration.FakePayment
            ? View(new FakePaymentViewModel($"https://{httpContextAccessor.HttpContext.Request.Host}{environment.EnvironmentName.ToReturnUrlPrefix()}/{form}/{path}/payment-response", reference, amount, form))
            : new NotFoundResult();
    }
}