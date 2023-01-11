using form_builder.Enum;
using form_builder.Helpers.EmailHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.EmailProvider;
using form_builder.Services.DocumentService;
using form_builder.Services.DocumentService.Entities;
using form_builder.Services.MappingService;
using form_builder.Providers.StorageProvider;
using form_builder.Providers.ReferenceNumbers;


namespace form_builder.Workflows.EmailWorkflow
{
    public class EmailWorkflow : IEmailWorkflow
    {
        private readonly IMappingService _mappingService;
        private readonly IEmailHelper _emailHelper;
        private readonly ISessionHelper _sessionHelper;
        private readonly IEmailProvider _emailProvider;
        private readonly IDocumentSummaryService _documentSummaryService;
        private readonly IReferenceNumberProvider _referenceNumberProvider;
        public EmailWorkflow(
            IMappingService mappingService,
            IEmailHelper emailHelper,
            ISessionHelper sessionHelper,
            IEmailProvider emailProvider,
            IReferenceNumberProvider referenceNumberProvider,
            IDocumentSummaryService documentSummaryService
            )
        {
            _mappingService = mappingService;
            _emailHelper = emailHelper;
            _sessionHelper = sessionHelper;
            _emailProvider = emailProvider;
            _referenceNumberProvider = referenceNumberProvider;
            _documentSummaryService = documentSummaryService;
        }

        public async Task<string> Submit(string form)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
                throw new ApplicationException($"A Session GUID was not provided.");

            var reference = string.Empty;

            var data = await _mappingService.Map(sessionGuid, form);

            if (data.BaseForm.GenerateReferenceNumber)
            {
                data.FormAnswers.CaseReference = _referenceNumberProvider.GetReference(data.BaseForm.ReferencePrefix);
                reference = data.FormAnswers.CaseReference;
            }

            var doc = _documentSummaryService.GenerateDocument(
               new DocumentSummaryEntity
               {
                   DocumentType = EDocumentType.Html,
                   PreviousAnswers = data.FormAnswers,
                   FormSchema = data.BaseForm
               });

            var body = System.Text.Encoding.Default.GetString(doc.Result);

            var email = _emailHelper.GetEmailInformation(form).Result;
            var emailMessage = new EmailMessage
                (email.Subject, body,
                "noreply@stockport.gov.uk",
                string.Join(",", email.To.ToArray())
                );

            var result =  _emailProvider.SendEmail(emailMessage).Result;

            if (result == System.Net.HttpStatusCode.OK)
            {
                return reference;
            }
            else
            {
                return "failure";
            }
            
        }
    }
}
