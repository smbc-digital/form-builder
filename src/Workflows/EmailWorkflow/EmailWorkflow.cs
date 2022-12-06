using form_builder.Helpers.Session;
using form_builder.Services.EmailService;
using form_builder.Services.MappingService;
using form_builder.Services.SubmitService;
using System.Net.Mail;
using StockportGovUK.NetStandard.Gateways.MailingService;
using StockportGovUK.NetStandard.Gateways.Models.Mail;

namespace form_builder.Workflows.EmailWorkflow
{
    public class EmailWorkflow : IEmailWorkflow
    {
        private readonly IEmailService _emailService;
        private readonly ISubmitService _submitService;
        private readonly IMappingService _mappingService;
        private readonly ISessionHelper _sessionHelper;
        private readonly IMailingServiceGateway _mailingServiceGateway;

        public EmailWorkflow(ISubmitService submitService,
            IMappingService mappingService,
            ISessionHelper sessionHelper,
            IEmailService emailService)
        {
            _submitService = submitService;
            _mappingService = mappingService;
            _sessionHelper = sessionHelper;
            _emailService = emailService;
            _mailingServiceGateway = new MailingServiceGateway(new HttpClient());
        }

        public async Task<string> Submit(string form)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
                throw new ApplicationException($"A Session GUID was not provided.");

            var data = await _mappingService.Map(sessionGuid, form);

            var emailMessage = new MailMessage
            {
                Subject = "Subject",
                Sender= new MailAddress("forms@stockport.gov.uk"),
                DeliveryNotificationOptions = DeliveryNotificationOptions.Never, 
                IsBodyHtml = true,
                Body = "Body"
            };

            emailMessage.To.Add("simon.estill@stockport.gov.uk");

            _mailingServiceGateway.Send(new MailModel { });

            return "success";
        }
    }
}
