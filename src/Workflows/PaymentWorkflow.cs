using System;
using System.Threading.Tasks;
using form_builder.Helpers.Session;
using form_builder.Services.MappingService;
using form_builder.Services.PayService;
using form_builder.Services.SubmtiService;

namespace form_builder.Workflows
{
    public interface IPaymentWorkflow
    {
        Task<string> Submit(string form, string path);
    }

    public class PaymentWorkflow : IPaymentWorkflow
    {
        private readonly ISubmitService _submitService;
        private readonly IMappingService _mappingService;
        private readonly IPayService _payService;
        private readonly ISessionHelper _sessionHelper;

        public PaymentWorkflow(IPayService payService, ISubmitService submitService, IMappingService mappingService, ISessionHelper sessionHelper)
        {
            _submitService = submitService;
            _mappingService = mappingService;
            _sessionHelper = sessionHelper;
            _payService = payService;
        }

        public async Task<string> Submit(string form, string path)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
            {
                throw new ApplicationException("A Session GUID was not provided.");
            }

            var data = await _mappingService.Map(sessionGuid, form);
            var paymentReference = await _submitService.PaymentSubmission(data, form, sessionGuid);

            return await _payService.ProcessPayment(data, form, path, paymentReference, sessionGuid);
        }
    }
}
