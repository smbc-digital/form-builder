using form_builder.Configuration;
using form_builder.Extensions;
using form_builder.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace form_builder.Controllers.Payment
{
    public class FakePaymentController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _environment;
        private readonly PaymentConfiguration _paymentConfiguration;

        public FakePaymentController(IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment, IOptions<PaymentConfiguration> paymentConfiguration)
        {
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            _paymentConfiguration = paymentConfiguration.Value;
        }

        [HttpGet]
        [Route("{form}/{path}/fake-payment")]
        public IActionResult Index([FromRoute] string form, [FromRoute] string path, [FromQuery] string reference, [FromQuery] string amount)
        {
            return _paymentConfiguration.FakePayment
                ? View(new FakePaymentViewModel($"https://{_httpContextAccessor.HttpContext.Request.Host}{_environment.EnvironmentName.ToReturnUrlPrefix()}/{form}/{path}/payment-response", reference, amount, form))
                : new NotFoundResult();
        }
    }
}