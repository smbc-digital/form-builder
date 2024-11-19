using form_builder.Helpers.Session;
using form_builder.Services.MappingService;
using form_builder.Services.PayService;
using form_builder.Services.SubmitService;

namespace form_builder.Workflows.PaymentWorkflow
{
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
            string browserSessionId = _sessionHelper.GetBrowserSessionId();
            string formSessionId = $"{form}::{browserSessionId}";

            if (string.IsNullOrEmpty(formSessionId))
                throw new ApplicationException("A Session GUID was not provided.");

            await _submitService.PreProcessSubmission(form, formSessionId);

            var data = await _mappingService.Map(formSessionId, form);
            var paymentReference = await _submitService.PaymentSubmission(data, form, formSessionId);

            return await _payService.ProcessPayment(data, form, path, paymentReference, formSessionId);
        }
    }
}