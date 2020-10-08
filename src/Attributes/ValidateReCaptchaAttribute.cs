using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using form_builder.Configuration;
using form_builder.Constants;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;

namespace form_builder.Attributes
{
    public class ValidateReCaptchaAttribute : ActionFilterAttribute
    {
        private readonly IGateway _gateway;
        private readonly ReCaptchaConfiguration _configuration;

        public ValidateReCaptchaAttribute(IGateway gateway, IOptions<ReCaptchaConfiguration> configuration)
        {
            _configuration = configuration.Value;
            _gateway = gateway;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var formData = (IDictionary<string, string[]>) context.ActionArguments["formData"];
            if (formData != null && !formData.ContainsKey("Submit"))
            {
                await base.OnActionExecutionAsync(context, next);
            }

            await DoReCaptchaValidation(context);
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
            var content = await response.Content.ReadAsStringAsync();

            var reCaptchaResponse = JsonConvert.DeserializeObject<ReCaptchaResponse>(content);
            if (reCaptchaResponse == null)
            {
                AddModelError(context, "Unable To Read Response From Server");
            }
            else if (!reCaptchaResponse.Success)
            {
                AddModelError(context, "Invalid reCaptcha");
            }
        }

        public class ReCaptchaResponse
        {
            public bool Success { get; set; }
        }
    }
}
