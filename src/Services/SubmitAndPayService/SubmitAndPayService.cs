using form_builder.Helpers.PageHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.SchemaProvider;
using form_builder.Providers.StorageProvider;
using form_builder.Services.SubmitService.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StockportGovUK.NetStandard.Gateways;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using StockportGovUK.NetStandard.Gateways.ComplimentsComplaintsServiceGateway;
using System.Dynamic;
using System.Linq;
using form_builder.Models.Elements;
using form_builder.Mappers;
using form_builder.Services.SubmitAndPayService.Entities;
using StockportGovUK.NetStandard.Gateways.Civica.Pay;
using StockportGovUK.NetStandard.Models.Civica.Pay.Request;
using form_builder.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using form_builder.Configuration;
using Microsoft.Extensions.Options;

namespace form_builder.Services.SubmitAndPayService
{
    public interface ISubmitAndPayService
    {
        Task<SubmitAndPayServiceEntity> ProcessSubmission(string form);
        Task<string> GeneratePaymentUrl(string reference, string form, string path);
    }

    public class SubmitAndPayService : ISubmitAndPayService
    {
        private readonly IDistributedCacheWrapper _distributedCache;
        private readonly ISchemaProvider _schemaProvider;
        private readonly IGateway _gateway;
        private readonly IComplimentsComplaintsServiceGateway _complimentsComplaintsServiceGateway;
        private readonly IPageHelper _pageHelper;
        private readonly ISessionHelper _sessionHelper;
        private readonly ILogger<SubmitAndPayService> _logger;
        private readonly ICivicaPayGateway _civicaPayGateway;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostingEnvironment _environment;
        private readonly CivicaPaymentConfiguration _paymentConfig;
        private readonly PaymentInformationConfiguration _paymentInformationConfig;

        public SubmitAndPayService(ILogger<SubmitAndPayService> logger, IDistributedCacheWrapper distributedCache, ISchemaProvider schemaProvider, IGateway gateway, IComplimentsComplaintsServiceGateway complimentsComplaintsServiceGateway, IPageHelper pageHelper, ISessionHelper sessionHelper, ICivicaPayGateway civicaPayGateway, IHttpContextAccessor httpContextAccessor, IHostingEnvironment environment, IOptions<CivicaPaymentConfiguration> paymentConfiguration, IOptions<PaymentInformationConfiguration> paymentInformationConfiguration)
        {
            _distributedCache = distributedCache;
            _schemaProvider = schemaProvider;
            _gateway = gateway;
            _complimentsComplaintsServiceGateway = complimentsComplaintsServiceGateway;
            _pageHelper = pageHelper;
            _sessionHelper = sessionHelper;
            _logger = logger;
            _civicaPayGateway = civicaPayGateway;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            _paymentConfig = paymentConfiguration.Value;
            _paymentInformationConfig = paymentInformationConfiguration.Value;
        }

        public async Task<SubmitAndPayServiceEntity> ProcessSubmission(string form)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
            {
                throw new ApplicationException($"A Session GUID was not provided.");
            }

            var baseForm = await _schemaProvider.Get<FormSchema>(form);
            var formData = _distributedCache.GetString(sessionGuid);
            var convertedAnswers = JsonConvert.DeserializeObject<FormAnswers>(formData);
            convertedAnswers.FormName = form;

            var currentPage = baseForm.GetPage(convertedAnswers.Path);
            var postUrl = currentPage.GetSubmitFormEndpoint(convertedAnswers);
            var postData = CreatePostData(convertedAnswers, baseForm);
            var reference = string.Empty;

            //hand off to veritn
            var response = await _gateway.PostAsync(postUrl, postData);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException($"SubmitService::ProcessSubmission, An exception has occured while attemping to call {postUrl}, Gateway responded with {response.StatusCode} status code, Message: {JsonConvert.SerializeObject(response)}");
            }

            if (response.Content != null)
            {
                var content = await response.Content.ReadAsStringAsync() ?? string.Empty;
                reference = JsonConvert.DeserializeObject<string>(content);
            }

            var page = baseForm.GetPage("success");

            if (page == null)
            {
                return new SubmitAndPayServiceEntity
                {
                    ViewName = "Submit",
                    ViewModel = convertedAnswers,
                    FeedbackFormUrl = baseForm.FeedbackForm
                };
            }

            var viewModel = await _pageHelper.GenerateHtml(page, new Dictionary<string, string>(), baseForm, sessionGuid);
            var success = new Success
            {
                FormName = baseForm.FormName,
                Reference = reference,
                FormAnswers = convertedAnswers,
                PageContent = viewModel.RawHTML,
                SecondaryHeader = page.Title
            };

            return new SubmitAndPayServiceEntity
            {
                ViewName = "Success",
                ViewModel = success,
                FeedbackFormUrl = baseForm.FeedbackForm
            };
        }

        public async Task<string> GeneratePaymentUrl(string reference, string form, string path)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid().ToString();
            var formData = _schemaProvider.Get(form);
            var pageData = formData.GetPage(path);
            var paymentInfo = _paymentInformationConfig.PaymentConfigs.Select(x => x).Where(c => c.FormName == form);
            var catalogueId = string.Empty; ;
            var accountReference = string.Empty;
            var amount = string.Empty; ;

            foreach (var payInfo in paymentInfo)
            {
                accountReference = payInfo.Settings.AccountReference;
                catalogueId = payInfo.Settings.CatalogueId;
                amount = payInfo.Settings.Amount;
            }

            var bucket = new CreateImmediateBasketRequest
            {
                CallingAppIdentifier = "Basket",
                CustomerID = _paymentConfig.CustomerId,
                ApiPassword = _paymentConfig.ApiPassword,
                ReturnURL = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}/{form}/{pageData.PageSlug}/payment-response",
                NotifyURL = string.Empty,
                CallingAppTranReference = reference,
                PaymentItems = new List<PaymentItem>
                {
                    new PaymentItem
                    {
                        PaymentDetails = new PaymentDetail
                        {
                            CatalogueID = catalogueId,
                            AccountReference = accountReference, 
                            PaymentAmount = amount, 
                            Quantity = "1",
                            PaymentNarrative = formData.FormName,
                            CallingAppTranReference = sessionGuid
                        },
                        AddressDetails = new AddressDetail()
                    }
                }
            };

            var civicaResponse = await _civicaPayGateway.CreateImmediateBasketAsync(bucket);

            if (civicaResponse.StatusCode != HttpStatusCode.OK)
                throw new System.Exception();

            //is this the right time and place to clear down the guid if an unsuccessful payment was made.
            _distributedCache.Remove(sessionGuid);
            _sessionHelper.RemoveSessionGuid();

            return _civicaPayGateway.GetPaymentUrl(civicaResponse.ResponseContent.BasketReference, civicaResponse.ResponseContent.BasketToken, sessionGuid);
        }

        private object CreatePostData(FormAnswers formAnswers, FormSchema formSchema)
        {
            var data = new ExpandoObject() as IDictionary<string, object>;

            formSchema.Pages.SelectMany(_ => _.ValidatableElements)
                .ToList()
                .ForEach(_ => data = RecursiveCheckAndCreate(string.IsNullOrEmpty(_.Properties.TargetMapping) ? _.Properties.QuestionId : _.Properties.TargetMapping, _, formAnswers, data));

            return data;
        }

        private IDictionary<string, object> RecursiveCheckAndCreate(string targetMapping, IElement element, FormAnswers formAnswers, IDictionary<string, object> obj)
        {
            var splitTargets = targetMapping.Split(".");

            if (splitTargets.Length == 1)
            {
                object objectValue;
                if (obj.TryGetValue(splitTargets[0], out objectValue))
                {
                    var combinedValue = $"{objectValue} {ElementMapper.GetAnswerValue(element, formAnswers)}";
                    obj.Remove(splitTargets[0]);
                    obj.Add(splitTargets[0], combinedValue);
                    return obj;
                }

                obj.Add(splitTargets[0], ElementMapper.GetAnswerValue(element, formAnswers));
                return obj;
            }

            object subObject;
            if (!obj.TryGetValue(splitTargets[0], out subObject))
                subObject = new ExpandoObject();

            subObject = RecursiveCheckAndCreate(targetMapping.Replace($"{splitTargets[0]}.", ""), element, formAnswers, subObject as IDictionary<string, object>);

            obj.Remove(splitTargets[0]);
            obj.Add(splitTargets[0], subObject);

            return obj;
        }
    }
}