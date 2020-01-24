using form_builder.Helpers.Session;
using form_builder.Services.MappingService;
using form_builder.Services.PayService;
using form_builder.Services.SubmtiService;
using System;
using System.Threading.Tasks;

namespace form_builder.Workflows
{
    public interface IPaymentWorkflow
    {
        Task<string> Submit(string form);
    }

    public class PaymentWorkflow : IPaymentWorkflow
    {
        private readonly ISubmitService _submitService;
        private readonly IMappingService _mappingService;
        private readonly IPayService _payService;
        private readonly ISessionHelper _sessionHelper;

        public PaymentWorkflow(IPayService _payService, ISubmitService submitService, IMappingService mappingService, ISessionHelper sessionHelper)
        {
            _submitService = submitService;
            _mappingService = mappingService;
            _sessionHelper = sessionHelper;
        }

        public async Task<string> Submit(string form)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
            {
                throw new ApplicationException($"A Session GUID was not provided.");
            }

            var data = await _mappingService.Map(sessionGuid, form);

            var paymentReference = await _payService.ProcessSubmission(data, form, sessionGuid);

            return await _payService.ProcessPayment(form, "", paymentReference, sessionGuid);
        }
    }
}
