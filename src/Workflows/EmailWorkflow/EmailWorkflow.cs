using form_builder.Enum;
using form_builder.Helpers.EmailHelpers;
using form_builder.Helpers.Session;
using form_builder.Models;
using form_builder.Providers.EmailProvider;
using form_builder.Services.DocumentService;
using form_builder.Services.DocumentService.Entities;
using form_builder.Services.MappingService;
using form_builder.Services.SubmitService;

namespace form_builder.Workflows.EmailWorkflow
{
    public class EmailWorkflow : IEmailWorkflow
    {
        private readonly IMappingService _mappingService;
        private readonly IEmailHelper _emailHelper;
        private readonly ISessionHelper _sessionHelper;
        private readonly IEmailProvider _emailProvider;
        private readonly IDocumentSummaryService _documentSummaryService;
        public EmailWorkflow(ISubmitService submitService,
            IMappingService mappingService,
            IEmailHelper emailHelper,
            ISessionHelper sessionHelper,
            IEmailProvider emailProvider,
            IDocumentSummaryService documentSummaryService
            )
        {
            _mappingService = mappingService;
            _emailHelper = emailHelper;
            _sessionHelper = sessionHelper;
            _emailProvider = emailProvider;
            _documentSummaryService = documentSummaryService;
        }

        public async Task<string> Submit(string form)
        {
            var sessionGuid = _sessionHelper.GetSessionGuid();

            if (string.IsNullOrEmpty(sessionGuid))
                throw new ApplicationException($"A Session GUID was not provided.");

            var data = await _mappingService.Map(sessionGuid, form);

            var doc = _documentSummaryService.GenerateDocument(
               new DocumentSummaryEntity
               {
                   DocumentType = EDocumentType.Txt,
                   PreviousAnswers = data.FormAnswers,
                   FormSchema = data.BaseForm
               });

            var body = System.Text.Encoding.Default.GetString(doc.Result);

            var email = _emailHelper.GetEmailInformation(form).Result;
            var emailMessage = new EmailMessage
                (email.Subject, body,
                "online.forms@stockport.gov.uk",
                string.Join(",", email.To.ToArray())
                );

            await _emailProvider.SendEmail(emailMessage);
            return "success";
        }
    }
}
