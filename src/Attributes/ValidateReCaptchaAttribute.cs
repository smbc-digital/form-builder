using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Controllers.Document;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StockportGovUK.NetStandard.Gateways;

namespace form_builder.Attributes
{
    public class ValidateReCaptchaAttribute : ActionFilterAttribute
    {
        private readonly IGateway _gateway;
        private readonly ReCaptchaConfiguration _configuration;
        private readonly ILogger<DocumentController> _logger;

        public ValidateReCaptchaAttribute(IGateway gateway, IOptions<ReCaptchaConfiguration> configuration, ILogger<DocumentController> logger)
        {
            _configuration = configuration.Value;
            _gateway = gateway;
            _logger = logger;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var formData = (IDictionary<string, string[]>) context.ActionArguments["formData"];
            if (context.ActionArguments["path"].Equals(FileUploadConstants.DOCUMENT_UPLOAD_URL_PATH) && formData != null && formData.ContainsKey("Submit"))
            {
                await DoReCaptchaValidation(context);
            }

            await base.OnActionExecutionAsync(context, next);
        }

        private static void AddModelError(ActionExecutingContext context, string error)
        {
            context.ModelState.AddModelError(ReCaptchaConstants.ReCaptchaModelErrorKey, error);
        }

        private async Task DoReCaptchaValidation(ActionExecutingContext context)
        {
            if (!context.HttpContext.Request.HasFormContentType)
            {
                AddModelError(context, "No reCaptcha Token Found");
                _logger.LogWarning("ValidateReCaptchaAttribute:: DoReCaptchaValidation:: No reCaptcha Token Found");
                return;
            }

            var token = context.HttpContext.Request.Form[ReCaptchaConstants.RecaptchaResponseTokenKey];

            if (string.IsNullOrWhiteSpace(token))
            {
                AddModelError(context, "Verify you are not a robot by selecting the verification box above the \"submit\" button");
            }
            else
            {
                await ValidateRecaptcha(context, token);
            }
        }

        private async Task ValidateRecaptcha(ActionExecutingContext context, string token)
        {
            var request = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", _configuration.AuthToken),
                new KeyValuePair<string, string>("response", token)
            });

            var response = await _gateway.PostAsync(_configuration.ApiVerificationEndpoint, request, false);
            
            if (response.Content == null)
            {
                AddModelError(context, "Unable To Read Response From Server");
                _logger.LogWarning("ValidateReCaptchaAttribute:: ValidateReCaptcha:: Unable To Read Response From Server");
            }
            else if (!response.IsSuccessStatusCode)
            {
                AddModelError(context, "Invalid reCaptcha");
                _logger.LogWarning("ValidateReCaptchaAttribute:: ValidateReCaptcha:: Invalid reCaptcha");
            }
        }
    }
}
