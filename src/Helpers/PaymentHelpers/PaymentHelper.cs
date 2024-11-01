using form_builder.Configuration;
using form_builder.Constants;
using form_builder.Helpers.Session;
using form_builder.Providers.Transforms.PaymentConfiguration;
using form_builder.Services.MappingService;
using form_builder.Services.MappingService.Entities;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;

namespace form_builder.Helpers.PaymentHelpers
{
    public class PaymentHelper : IPaymentHelper
    {
        private readonly IGateway _gateway;
        private readonly IPaymentConfigurationTransformDataProvider _paymentConfigProvider;
        private readonly ISessionHelper _sessionHelper;
        private readonly IMappingService _mappingService;
        private readonly IWebHostEnvironment _hostingEnvironment;


        public PaymentHelper(
            IGateway gateway,
            ISessionHelper sessionHelper,
            IMappingService mappingService,
            IWebHostEnvironment hostingEnvironment,
            IPaymentConfigurationTransformDataProvider paymentConfigProvider)
        {
            _gateway = gateway;
            _sessionHelper = sessionHelper;
            _mappingService = mappingService;
            _hostingEnvironment = hostingEnvironment;
            _paymentConfigProvider = paymentConfigProvider;
        }

        public async Task<PaymentInformation> GetFormPaymentInformation(string form)
        {
            var sessionGuid = _sessionHelper.GetBrowserSessionId();
            var mappingEntity = await _mappingService.Map(sessionGuid, form);
            if (mappingEntity is null)
                throw new Exception($"PayService:: No mapping entity found for {form}");

            var paymentConfig = await _paymentConfigProvider.Get<List<PaymentInformation>>();
            var formPaymentConfig = paymentConfig.FirstOrDefault(_ => _.FormName.Any(_ => _.Equals(form)));

            if (formPaymentConfig is null)
                throw new Exception($"PayService:: No payment information found for {form}");

            if (!string.IsNullOrEmpty(formPaymentConfig.Settings.AddressReference))
                formPaymentConfig.Settings.AddressReference = formPaymentConfig.Settings.AddressReference.Insert(formPaymentConfig.Settings.AddressReference.Length - 2, AddressConstants.DESCRIPTION_SUFFIX);

            if (string.IsNullOrEmpty(formPaymentConfig.Settings.Amount))
                formPaymentConfig.Settings.Amount = await GetPaymentAmountAsync(mappingEntity, formPaymentConfig);

            return formPaymentConfig;
        }

        private async Task<string> GetPaymentAmountAsync(MappingEntity formData, PaymentInformation formPaymentConfig)
        {
            try
            {
                var postUrl = formPaymentConfig.Settings.CalculationSlug;

                if (postUrl.URL is null || postUrl.AuthToken is null)
                    throw new Exception($"PayService::CalculateAmountAsync, slug for {_hostingEnvironment.EnvironmentName} not found or incomplete");

                _gateway.ChangeAuthenticationHeader(postUrl.AuthToken);
                var response = await _gateway.PostAsync(postUrl.URL, formData.Data);

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"PayService::CalculateAmountAsync, Gateway returned unsuccessful status code {response.StatusCode}, Response: {Newtonsoft.Json.JsonConvert.SerializeObject(response)}");

                if (response.Content is null)
                    throw new ApplicationException($"PayService::CalculateAmountAsync, Gateway {postUrl.URL} responded with null content");

                var content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                    throw new ApplicationException($"PayService::CalculateAmountAsync, Gateway {postUrl.URL} responded with empty payment amount within content");

                return JsonConvert.DeserializeObject<string>(content);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
